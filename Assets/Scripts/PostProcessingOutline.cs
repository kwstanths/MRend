using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(OutlineRenderer), PostProcessEvent.BeforeTransparent, "Custom/Outline")]
public sealed class PostProcessingOutline : PostProcessEffectSettings
{
    [Tooltip("Thickness")]
    public IntParameter thickness = new IntParameter { value = 3 };
}

public sealed class OutlineRenderer : PostProcessEffectRenderer<PostProcessingOutline>
{
    public override void Render(PostProcessRenderContext context)
    {
        var sheet = context.propertySheets.Get(Shader.Find("Hidden/Custom/Outline"));
        sheet.properties.SetFloat("_Thickness", settings.thickness);
        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}