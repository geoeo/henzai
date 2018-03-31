using System;
using System.Numerics;
using Veldrid;

namespace Henzai.Runtime.Render
{
    ///<summary>
    /// Encapsulates Resources Needed at Scene Level for Rendering at Runtime.
    /// Although Resources are at Scene Level, the data may be set per Model.
    /// Veldrid Resources are Managed By <see cref="DisposeCollectorResourceFactory"/>
    ///</summary>
    public sealed class SceneRuntimeState
   {
       /// <summary>
       /// Buffer Encapsulates CPU Memory for MVP Matrix
       /// </summary>
       public DeviceBuffer CameraProjViewBuffer {get; set;}

       /// <summary>
       /// Buffer Encapsulates CPU Memory for Light Information
       /// </summary>
       public DeviceBuffer LightBuffer {get; set;}
       /// <summary>
       /// Buffer Encapsulates CPU Memory for Material
       /// </summary>
       public DeviceBuffer MaterialBuffer {get; set;}

       /// <summary>
       /// Encapsulates GPU Memory Layout. See:
       /// <see cref="Veldrid.ResourceSet"/>
       /// </summary>
       public ResourceSet CameraResourceSet {get;set;}
       /// <summary>
       /// Encapsulates GPU Memory Layout. See:
       /// <see cref="Veldrid.ResourceSet"/>
       /// </summary>
       public ResourceSet LightResourceSet {get;set;}
       /// <summary>
       /// Encapsulates GPU Memory Layout. See:
       /// <see cref="Veldrid.ResourceSet"/>
       /// </summary>
       public ResourceSet MaterialResourceSet {get;set;}
       /// <summary>
       /// Encapsulates Lighting Layout.
       /// </summary>
       public Light Light {get;set;}

   } 
}