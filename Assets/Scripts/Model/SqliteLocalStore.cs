using System;
using System.Collections.Generic;
using System.IO;
using SQLite;
using UnityEngine;

/// <summary>
/// 用途：统一管理本地 SQLite 数据库的增删改查。返回：按方法说明。
/// </summary>
public static class SqliteLocalStore
{
    private static readonly object sLock = new object();
    private static bool sIsInitialized;
    private static string sDatabasePath;
    private static SQLiteConnection sConnection;

    /// <summary>
    /// 用途：启动时打开数据库并建表。返回：是否初始化成功。
    /// </summary>
    public static bool Initialize()
    {
        lock (sLock)
        {
            if (sIsInitialized)
            {
                return true;
            }

            try
            {
                EnsureInitialized();
                return sIsInitialized;
            }
            catch (Exception exception)
            {
                Debug.LogError($"SqliteLocalStore.Initialize failed.\n{exception}");
                sIsInitialized = false;
                if (sConnection != null)
                {
                    sConnection.Dispose();
                    sConnection = null;
                }

                return false;
            }
        }
    }

    /// <summary>
    /// 用途：是否已完成初始化。返回：已初始化为 true。
    /// </summary>
    public static bool IsInitialized => sIsInitialized;

    /// <summary>
    /// 用途：获取当前 SQLite 数据库文件的完整路径。返回：绝对路径字符串。
    /// </summary>
    public static string GetDatabasePath()
    {
        EnsureInitialized();
        return sDatabasePath;
    }

    /// <summary>
    /// 用途：在指定集合中新增记录；同键已存在时返回 false。返回：是否新增成功。
    /// </summary>
    public static bool Create(string collection, string key, string value)
    {
        if (!TryValidateCollectionAndKey(collection, key))
        {
            return false;
        }

        lock (sLock)
        {
            EnsureInitialized();
            if (Exists(collection, key))
            {
                Debug.LogWarning($"SqliteLocalStore.Create skipped, key already exists: {collection}/{key}");
                return false;
            }

            var utcNow = DateTime.UtcNow.ToString("o");
            var affected = sConnection.Execute(
                $"INSERT INTO {GameDefine.LocalSqliteCollectionTable} (collection, record_key, json_value, created_utc, updated_utc) VALUES (?, ?, ?, ?, ?)",
                collection,
                key,
                value ?? string.Empty,
                utcNow,
                utcNow);
            return affected > 0;
        }
    }

    /// <summary>
    /// 用途：在指定集合中新增对象记录（序列化为 JSON）。返回：是否新增成功。
    /// </summary>
    public static bool Create<T>(string collection, string key, T value)
    {
        return Create(collection, key, JsonUtility.ToJson(value));
    }

    /// <summary>
    /// 用途：读取指定集合中的字符串值。返回：值；不存在时返回 null。
    /// </summary>
    public static string Read(string collection, string key)
    {
        if (!TryValidateCollectionAndKey(collection, key))
        {
            return null;
        }

        lock (sLock)
        {
            EnsureInitialized();
            return sConnection.ExecuteScalar<string>(
                $"SELECT json_value FROM {GameDefine.LocalSqliteCollectionTable} WHERE collection = ? AND record_key = ? LIMIT 1",
                collection,
                key);
        }
    }

    /// <summary>
    /// 用途：读取并反序列化指定集合中的对象。返回：是否读取成功。
    /// </summary>
    public static bool TryRead<T>(string collection, string key, out T value)
    {
        value = default;
        var json = Read(collection, key);
        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonUtility.FromJson<T>(json);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError($"SqliteLocalStore.TryRead failed for {collection}/{key}\n{exception}");
            return false;
        }
    }

    /// <summary>
    /// 用途：更新指定集合中的记录；键不存在时返回 false。返回：是否更新成功。
    /// </summary>
    public static bool Update(string collection, string key, string value)
    {
        if (!TryValidateCollectionAndKey(collection, key))
        {
            return false;
        }

        lock (sLock)
        {
            EnsureInitialized();
            if (!Exists(collection, key))
            {
                Debug.LogWarning($"SqliteLocalStore.Update skipped, key not found: {collection}/{key}");
                return false;
            }

            var affected = sConnection.Execute(
                $"UPDATE {GameDefine.LocalSqliteCollectionTable} SET json_value = ?, updated_utc = ? WHERE collection = ? AND record_key = ?",
                value ?? string.Empty,
                DateTime.UtcNow.ToString("o"),
                collection,
                key);
            return affected > 0;
        }
    }

    /// <summary>
    /// 用途：更新指定集合中的对象记录（序列化为 JSON）。返回：是否更新成功。
    /// </summary>
    public static bool Update<T>(string collection, string key, T value)
    {
        return Update(collection, key, JsonUtility.ToJson(value));
    }

    /// <summary>
    /// 用途：写入或覆盖记录（不存在则创建，存在则更新）。返回：是否保存成功。
    /// </summary>
    public static bool Upsert(string collection, string key, string value)
    {
        if (!TryValidateCollectionAndKey(collection, key))
        {
            return false;
        }

        return Exists(collection, key)
            ? Update(collection, key, value)
            : Create(collection, key, value);
    }

    /// <summary>
    /// 用途：写入或覆盖对象记录（序列化为 JSON）。返回：是否保存成功。
    /// </summary>
    public static bool Upsert<T>(string collection, string key, T value)
    {
        return Upsert(collection, key, JsonUtility.ToJson(value));
    }

    /// <summary>
    /// 用途：删除指定集合中的一条记录。返回：是否删除成功。
    /// </summary>
    public static bool Delete(string collection, string key)
    {
        if (!TryValidateCollectionAndKey(collection, key))
        {
            return false;
        }

        lock (sLock)
        {
            EnsureInitialized();
            var affected = sConnection.Execute(
                $"DELETE FROM {GameDefine.LocalSqliteCollectionTable} WHERE collection = ? AND record_key = ?",
                collection,
                key);
            return affected > 0;
        }
    }

    /// <summary>
    /// 用途：删除指定集合下的全部记录。返回：删除条数。
    /// </summary>
    public static int DeleteCollection(string collection)
    {
        if (string.IsNullOrWhiteSpace(collection))
        {
            Debug.LogWarning("SqliteLocalStore.DeleteCollection: collection is null or empty.");
            return 0;
        }

        lock (sLock)
        {
            EnsureInitialized();
            return sConnection.Execute(
                $"DELETE FROM {GameDefine.LocalSqliteCollectionTable} WHERE collection = ?",
                collection);
        }
    }

    /// <summary>
    /// 用途：判断指定集合中是否存在某键。返回：存在为 true。
    /// </summary>
    public static bool Exists(string collection, string key)
    {
        if (!TryValidateCollectionAndKey(collection, key))
        {
            return false;
        }

        lock (sLock)
        {
            EnsureInitialized();
            var count = sConnection.ExecuteScalar<int>(
                $"SELECT COUNT(1) FROM {GameDefine.LocalSqliteCollectionTable} WHERE collection = ? AND record_key = ?",
                collection,
                key);
            return count > 0;
        }
    }

    /// <summary>
    /// 用途：列出指定集合下的全部键名。返回：键名列表。
    /// </summary>
    public static List<string> ListKeys(string collection)
    {
        var keys = new List<string>();
        if (string.IsNullOrWhiteSpace(collection))
        {
            return keys;
        }

        lock (sLock)
        {
            EnsureInitialized();
            var rows = sConnection.Query<RecordKeyRow>(
                $"SELECT record_key FROM {GameDefine.LocalSqliteCollectionTable} WHERE collection = ? ORDER BY record_key",
                collection);
            for (var i = 0; i < rows.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(rows[i].record_key))
                {
                    keys.Add(rows[i].record_key);
                }
            }
        }

        return keys;
    }

    /// <summary>
    /// 用途：执行自定义非查询 SQL（INSERT/UPDATE/DELETE）。返回：受影响行数。
    /// </summary>
    public static int ExecuteNonQuery(string sql, params object[] args)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            Debug.LogWarning("SqliteLocalStore.ExecuteNonQuery: sql is null or empty.");
            return 0;
        }

        lock (sLock)
        {
            EnsureInitialized();
            return sConnection.Execute(sql, args);
        }
    }

    /// <summary>
    /// 用途：执行自定义查询并返回标量结果。返回：标量值。
    /// </summary>
    public static T ExecuteScalar<T>(string sql, params object[] args)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            Debug.LogWarning("SqliteLocalStore.ExecuteScalar: sql is null or empty.");
            return default;
        }

        lock (sLock)
        {
            EnsureInitialized();
            return sConnection.ExecuteScalar<T>(sql, args);
        }
    }

    /// <summary>
    /// 用途：关闭数据库连接（如切场景或退出前）。返回：无。
    /// </summary>
    public static void Close()
    {
        lock (sLock)
        {
            if (sConnection == null)
            {
                sIsInitialized = false;
                return;
            }

            sConnection.Close();
            sConnection.Dispose();
            sConnection = null;
            sIsInitialized = false;
        }
    }

    private static void EnsureInitialized()
    {
        if (sIsInitialized)
        {
            return;
        }

        sDatabasePath = Path.Combine(Application.persistentDataPath, GameDefine.LocalSqliteFileName);
        var directory = Path.GetDirectoryName(sDatabasePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        sConnection = new SQLiteConnection(sDatabasePath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
        sConnection.Execute($"PRAGMA foreign_keys = ON;");
        sConnection.Execute(
            $@"CREATE TABLE IF NOT EXISTS {GameDefine.LocalSqliteCollectionTable} (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                collection TEXT NOT NULL,
                record_key TEXT NOT NULL,
                json_value TEXT NOT NULL DEFAULT '',
                created_utc TEXT NOT NULL,
                updated_utc TEXT NOT NULL,
                UNIQUE(collection, record_key)
            );");
        sConnection.Execute(
            $@"CREATE INDEX IF NOT EXISTS idx_{GameDefine.LocalSqliteCollectionTable}_collection
               ON {GameDefine.LocalSqliteCollectionTable}(collection);");

        sIsInitialized = true;
        Debug.Log($"SqliteLocalStore initialized: {sDatabasePath}");
    }

    private static bool TryValidateCollectionAndKey(string collection, string key)
    {
        if (string.IsNullOrWhiteSpace(collection))
        {
            Debug.LogWarning("SqliteLocalStore: collection is null or empty.");
            return false;
        }

        if (!string.IsNullOrWhiteSpace(key))
        {
            return true;
        }

        Debug.LogWarning("SqliteLocalStore: key is null or empty.");
        return false;
    }

    private sealed class RecordKeyRow
    {
        public string record_key { get; set; }
    }
}
