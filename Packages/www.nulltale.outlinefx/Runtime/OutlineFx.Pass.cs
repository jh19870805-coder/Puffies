using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//  OutlineFx © NullTale - https://x.com/NullTale/
namespace OutlineFx
{
    public partial class OutlineFxFeature
    {
        private static readonly int s_Alpha   = Shader.PropertyToID("_Alpha");
        private static readonly int s_MainTex = Shader.PropertyToID("_MainTex");
        private static readonly int s_Step    = Shader.PropertyToID("_Step");
        private static readonly int s_Color   = Shader.PropertyToID("_Color");
        private static readonly int s_Solid   = Shader.PropertyToID("_Solid");
        private static readonly int s_GapStep = Shader.PropertyToID("_GapStep");
        
        private static readonly int s_AlphaTex    = Shader.PropertyToID("_AlphaTex");
        private static readonly int s_AlphaTO = Shader.PropertyToID("_AlphaTO");
        
        private class Pass : ScriptableRenderPass
        {
            public OutlineFxFeature _owner;
            
            private FilteringSettings       _filtering;
            private RenderStateBlock        _override;
            private RenderTarget            _buffer;
            private RenderTarget            _unionBuffer;
            private RenderTarget            _gapBuffer;
            private RenderTarget            _closedBuffer;
            private RenderTarget            _activeGapBuffer;
            private RenderTarget            _activeClosedBuffer;
            private RenderTarget            _activeExpandedBuffer;
            private RenderTarget            _outlineBuffer;
            private RenderTarget            _outputTex;
            private RTHandle                _output;

            // =======================================================================
            public void Init()
            {
                renderPassEvent = _owner._event;
                _buffer       = new RenderTarget().Allocate(nameof(_buffer));
                _unionBuffer  = new RenderTarget().Allocate(nameof(_unionBuffer));
                _gapBuffer    = new RenderTarget().Allocate(nameof(_gapBuffer));
                _closedBuffer = new RenderTarget().Allocate(nameof(_closedBuffer));
                _activeGapBuffer = new RenderTarget().Allocate(nameof(_activeGapBuffer));
                _activeClosedBuffer = new RenderTarget().Allocate(nameof(_activeClosedBuffer));
                _activeExpandedBuffer = new RenderTarget().Allocate(nameof(_activeExpandedBuffer));
                _outlineBuffer = new RenderTarget().Allocate(nameof(_outlineBuffer));
                if (_owner._output.Enabled)
                    _outputTex = new RenderTarget().Allocate(_owner._output.value);
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (_owner._ensureMaterialReady() == false)
                    return;

                var outlineMaterial = _owner._outlineMat;
                var transparentPass = outlineMaterial.FindPass("Transparent");
                var outlinePass = outlineMaterial.FindPass("Outline");
                var dilateHorizontalPass = outlineMaterial.FindPass("DilateMaskHorizontal");
                var dilateVerticalPass = outlineMaterial.FindPass("DilateMaskVertical");
                var erodeHorizontalPass = outlineMaterial.FindPass("ErodeMaskHorizontal");
                var erodeVerticalPass = outlineMaterial.FindPass("ErodeMaskVertical");
                var expandHorizontalPass = outlineMaterial.FindPass("ExpandMaskHorizontal");
                var expandVerticalPass = outlineMaterial.FindPass("ExpandMaskVertical");
                var maskInteriorPass = outlineMaterial.FindPass("MaskInteriorOutline");
                if (transparentPass < 0
                    || outlinePass < 0
                    || dilateHorizontalPass < 0
                    || dilateVerticalPass < 0
                    || erodeHorizontalPass < 0
                    || erodeVerticalPass < 0
                    || expandHorizontalPass < 0
                    || expandVerticalPass < 0
                    || maskInteriorPass < 0)
                {
                    return;
                }

                // allocate resources
                var cmd  = CommandBufferPool.Get(nameof(OutlineFxFeature));
                var desc = renderingData.cameraData.cameraTargetDescriptor;
                desc.colorFormat = RenderTextureFormat.ARGB32;
                _buffer.Get(cmd, desc);
				_unionBuffer.Get(cmd, desc);
				_gapBuffer.Get(cmd, desc);
				_closedBuffer.Get(cmd, desc);
				_activeGapBuffer.Get(cmd, desc);
				_activeClosedBuffer.Get(cmd, desc);
				_activeExpandedBuffer.Get(cmd, desc);
				_outlineBuffer.Get(cmd, desc);
		
                outlineMaterial.SetFloat(s_Alpha, _owner._alphaCutout);
                outlineMaterial.SetFloat(s_Solid, _owner._solid);
                
                if (_owner._solidMask._enabled)
                {
                    var sm = _owner._solidMask;
                    outlineMaterial.SetTexture(s_AlphaTex, sm._pattern);
                    var xPeriod = 1f / (sm._velocity.x / 1000f);
                    var yPeriod = 1f / (sm._velocity.y / 1000f);
                    var xOffset = sm._velocity.x == 0 ? 0 : (Time.unscaledTime % xPeriod) / xPeriod * sm._scale;
                    var yOffset = sm._velocity.y == 0 ? 0 : (Time.unscaledTime % yPeriod) / yPeriod * sm._scale;
                    
                    var aspectTex = sm._pattern.width / (float)sm._pattern.height;
                    
                    outlineMaterial.SetVector(s_AlphaTO, new Vector4(sm._scale * (Screen.width / (float)Screen.height) / aspectTex, sm._scale, xOffset, yOffset));
                }
                
#if !UNITY_2022_1_OR_NEWER
                if (_owner._output.Enabled == false)
                    _output = RTHandles.Alloc(renderingData.cameraData.renderer.cameraColorTarget);
#else
				_output = renderingData.cameraData.renderer.cameraColorTargetHandle;
#endif

                if (_owner._output.Enabled)
                {
                    _outputTex.Get(cmd, desc);
                    cmd.SetRenderTarget(_outputTex.Handle);
                    cmd.ClearRenderTarget(false, true, Color.clear, 1f);
                }

#if !UNITY_2022_1_OR_NEWER
                var maskDepth = renderingData.cameraData.renderer.cameraDepthTarget == BuiltinRenderTextureType.CameraTarget
                    ? renderingData.cameraData.renderer.cameraColorTarget
                    : renderingData.cameraData.renderer.cameraDepthTarget;
#else
                var maskDepth = renderingData.cameraData.renderer.cameraDepthTargetHandle;
#endif
                
                SetMaskTarget(_buffer.Handle);
                cmd.ClearRenderTarget(false, true, Color.clear, 1f);
                foreach (var inst in _renderers)
                {
                    if (inst == null || inst is OutlineBlocker)
                        continue;

                    DrawMask(inst, inst.Color);
                }

                SetMaskTarget(_unionBuffer.Handle);
                cmd.ClearRenderTarget(false, true, Color.clear, 1f);
                foreach (var inst in _renderers)
                {
                    if (inst != null)
                        DrawMask(inst, Color.white);
                }
                _renderers.Clear();

                cmd.SetGlobalVector(
                    s_GapStep,
                    new Vector4(1f / desc.width, 1f / desc.height, 0f, 0f));
                _blit(_unionBuffer.Handle, _gapBuffer.Handle, outlineMaterial, dilateHorizontalPass);
                _blit(_gapBuffer.Handle, _closedBuffer.Handle, outlineMaterial, dilateVerticalPass);
                _blit(_closedBuffer.Handle, _gapBuffer.Handle, outlineMaterial, erodeHorizontalPass);
                _blit(_gapBuffer.Handle, _closedBuffer.Handle, outlineMaterial, erodeVerticalPass);

                _blit(_buffer.Handle, _activeGapBuffer.Handle, outlineMaterial, dilateHorizontalPass);
                _blit(_activeGapBuffer.Handle, _activeClosedBuffer.Handle, outlineMaterial, dilateVerticalPass);
                _blit(_activeClosedBuffer.Handle, _activeGapBuffer.Handle, outlineMaterial, erodeHorizontalPass);
                _blit(_activeGapBuffer.Handle, _activeClosedBuffer.Handle, outlineMaterial, erodeVerticalPass);

                _blit(_activeClosedBuffer.Handle, _activeGapBuffer.Handle, outlineMaterial, expandHorizontalPass);
                _blit(_activeGapBuffer.Handle, _activeExpandedBuffer.Handle, outlineMaterial, expandVerticalPass);
                _blit(_closedBuffer.Handle, _gapBuffer.Handle, outlineMaterial, expandHorizontalPass);
                _blit(_gapBuffer.Handle, _unionBuffer.Handle, outlineMaterial, expandVerticalPass);

                cmd.SetGlobalVector(s_Step, _owner._step);
                cmd.SetRenderTarget(_outlineBuffer.Handle);
                cmd.ClearRenderTarget(false, true, Color.clear, 1f);
                _blit(_activeExpandedBuffer.Handle, _outlineBuffer.Handle, outlineMaterial, outlinePass);
                cmd.SetGlobalTexture(s_MaskTexId, _unionBuffer.Handle.nameID);
                _blit(
                    _outlineBuffer.Handle,
                    _owner._output.Enabled ? _outputTex.Handle : _output,
                    outlineMaterial,
                    maskInteriorPass);

                _execute();
				
                // -----------------------------------------------------------------------
                void SetMaskTarget(RTHandle target)
                {
                    if (_owner._attachDepth)
                    {
                        cmd.SetRenderTarget(target, maskDepth);
                    }
                    else
                    {
                        cmd.SetRenderTarget(target);
                    }
                }

                void DrawMask(Outline inst, Color color)
                {
                    for (var i = 0; i < inst._renderer.sharedMaterials.Length; i++)
                    {
                        cmd.SetGlobalTexture(s_MainTex, inst._renderer.sharedMaterials[i].mainTexture);
                        cmd.SetGlobalColor(s_Color, color);
                        cmd.DrawRenderer(inst._renderer, outlineMaterial, i, transparentPass);
                    }
                }

                void _blit(RTHandle from, RTHandle to, Material mat, int pass = 0)
                {
                    OutlineFxFeature._blit(cmd, from, to, mat, pass);
                }

                void _execute()
                {
                    context.ExecuteCommandBuffer(cmd);
                    CommandBufferPool.Release(cmd);
                }
            }

            public override void FrameCleanup(CommandBuffer cmd)
            {
                _buffer.Release(cmd);
				_unionBuffer.Release(cmd);
				_gapBuffer.Release(cmd);
				_closedBuffer.Release(cmd);
				_activeGapBuffer.Release(cmd);
				_activeClosedBuffer.Release(cmd);
				_activeExpandedBuffer.Release(cmd);
				_outlineBuffer.Release(cmd);
/*                
#if !UNITY_2022_1_OR_NEWER
                RTHandles.Release(_output);
#else
                if (_owner._output.Enabled)
                    RTHandles.Release(_output);
#endif*/
            }
        }
    }
}
