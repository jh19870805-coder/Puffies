#ifndef EGG_URP_GPUSKELETONFUNCTION_INCLUDE
#define EGG_URP_GPUSKELETONFUNCTION_INCLUDE

float4 GetBoneMatrixLine(int pixel_index, Texture2D _AnimationMap, float4 _AnimationMap_TexelSize)
{
    int pixel_y = pixel_index * _AnimationMap_TexelSize.x;
    int pixel_x = pixel_index - pixel_y * _AnimationMap_TexelSize.z;
    float4 matrix_line = _AnimationMap.Load(int3(pixel_x, pixel_y, 0));
    return matrix_line;
}

float4x4 GetBoneMatrix(int pixel_index, Texture2D _AnimationMap, float4 _AnimationMap_TexelSize)
{
    float4 m0 = GetBoneMatrixLine(pixel_index, _AnimationMap, _AnimationMap_TexelSize);
    float4 m1 = GetBoneMatrixLine(pixel_index + 1, _AnimationMap, _AnimationMap_TexelSize);
    float4 m2 = GetBoneMatrixLine(pixel_index + 2, _AnimationMap, _AnimationMap_TexelSize);
    return float4x4(m0, m1, m2, float4(0, 0, 0, 1));
}

float4 GetVertexPos(float4 positionOS, float2 v_index, float2 v_weight, Texture2D _AnimationMap, float4 _AnimationMap_TexelSize, float4 param, float4 frame_param)
{
    // 帧率
    float frame_rate = param.x;
    // 骨骼数量 * 3（每个骨骼矩阵占3个像素行）
    float pixel_per_frame = param.y;
    // 动作帧长度，用于取模
    half section = param.z;
    // 默认只有1个动作，并且循环播放，可简化计算
    float start_time = frame_param.x;
    float elapse_frame = (_Time.y - start_time) * frame_rate;
    uint final_elapse_frame = fmod(elapse_frame, section);
    int pixel_index = pixel_per_frame * final_elapse_frame;
    // 特效使用的模型，其顶点只收1根骨骼影响。精度可接受，优化计算量
    int bone1_index = pixel_index + v_index.x * 3;
    float4x4 mat1 = GetBoneMatrix(bone1_index, _AnimationMap, _AnimationMap_TexelSize);
    return mul(mat1, positionOS);
    // 如果需要开启两根骨骼加权计算，则使用下面的代码
    /*
    int bone2_index = pixel_index + v_index.y * 3;
    float4x4 mat2 = GetBoneMatrix(bone2_index, _AnimationMap, _AnimationMap_TexelSize);
    float4 pos = mul(mat1, positionOS) * v_weight.x + mul(mat2, positionOS) * (1 - v_weight.x);
    return pos;
    */
}

/*
float4 GetVertexPos(float4 positionOS, float2 v_index, float2 v_weight, inout float3 normalOS, inout float3 tangentOS)
{
    float4x4 mat1 = GetVertexPos(positionOS, v_index);
    // 加权计算出最终位置
    float4 pos = mul(mat1, positionOS) ;//* v_weight.x + mul(mat2, positionOS) * (1 - v_weight.x);
    normalOS = mul((float3x3)mat1, normalOS) ;//* v_weight.x + mul((float3x3)mat2, normalOS) * (1 - v_weight.x);
    tangentOS = mul((float3x3)mat1, tangentOS) ;//* v_weight.x + mul((float3x3)mat2, tangentOS) * (1 - v_weight.x);

    return pos;
}

float4 GetVertexPos(float4 positionOS, float2 v_index, float2 v_weight)
{
    float4x4 mat1 = GetVertexPos(positionOS, v_index);
    // 加权计算出最终位置
    float4 pos = mul(mat1, positionOS);// * v_weight.x + mul(mat2, positionOS) * (1 - v_weight.x);
    return pos;
}
*/

#endif