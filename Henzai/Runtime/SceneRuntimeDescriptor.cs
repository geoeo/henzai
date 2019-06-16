using System;
using System.Numerics;
using Henzai.Cameras;
using Veldrid;


namespace Henzai.Runtime
{
    ///<summary>
    /// Encapsulates Resources Needed at Scene Level for Rendering at Runtime.
    /// Although Resources are at Scene Level, the data may be set per Model.
    /// Veldrid Resources are Managed By <see cref="DisposeCollectorResourceFactory"/>
    ///</summary>
    public sealed class SceneRuntimeDescriptor
   {
       public ResourceSet[] NO_RESOURCE_SET {get; private set;}
       /// <summary>
       /// Buffer Encapsulates CPU Memory for MVP Matrix
       /// </summary>
       public DeviceBuffer CameraProjViewBuffer {get; set;}

       /// <summary>
       /// Buffer Encapsulates CPU Memory for Light Information
       /// </summary>
       public DeviceBuffer LightBuffer {get; set;}
       /// <summary>
       /// Buffer Encapsulates CPU Memory for Point Light Information
       /// </summary>
       public DeviceBuffer SpotLightBuffer {get; set;}
       /// <summary>
       /// Buffer Encapsulates CPU Memory for Material
       /// </summary>
       public DeviceBuffer MaterialBuffer {get; set;}
       /// <summary>
       /// Buffer Encapsulates CPU Memory for a VP Matrix
       /// </summary>
       public DeviceBuffer LightProvViewBuffer {get; set;}
       /// <summary>
       /// Encapsulates GPU Memory Layout. See:
       /// <see cref="Veldrid.ResourceLayout"/>
       /// </summary>
       public ResourceLayout CameraResourceLayout {get; set;}

       /// <summary>
       /// Encapsulates GPU Memory Layout. See:
       /// <see cref="Veldrid.ResourceLayout"/>
       /// </summary>
       public ResourceLayout LightResourceLayout {get; set;}
       /// <summary>
       /// Encapsulates GPU Memory Layout. See:
       /// <see cref="Veldrid.ResourceLayout"/>
       /// </summary>
       public ResourceLayout SpotLightResourceLayout {get; set;}
       /// <summary>
       /// Encapsulates GPU Memory Layout. See:
       /// <see cref="Veldrid.ResourceLayout"/>
       /// </summary>
       public ResourceLayout MaterialResourceLayout {get; set;}
       /// <summary>
       /// Encapsulates GPU Memory Layout. See:
       /// <see cref="Veldrid.ResourceLayout"/>
       /// </summary>
       public ResourceLayout LightProvViewResourceLayout {get; set;}

       /// <summary>
       /// Encapsulates GPU Memory Layout and Resource. See:
       /// <see cref="Veldrid.ResourceSet"/>
       /// <see cref="Veldrid.BindableResource"/>
       /// </summary>
       public ResourceSet CameraResourceSet {get;set;}
       /// <summary>
       /// Encapsulates GPU Memory Layout and Resource. See:
       /// <see cref="Veldrid.ResourceSet"/>
       /// <see cref="Veldrid.BindableResource"/>
       /// </summary>
       public ResourceSet LightResourceSet {get;set;}
       /// <summary>
       /// Encapsulates GPU Memory Layout and Resource. See:
       /// <see cref="Veldrid.ResourceSet"/>
       /// <see cref="Veldrid.BindableResource"/>
       /// </summary>
       public ResourceSet SpotLightResourceSet {get;set;}
       /// <summary>
       /// Encapsulates GPU Memory Layout and Resource. See:
       /// <see cref="Veldrid.ResourceSet"/>
       /// <see cref="Veldrid.BindableResource"/>
       /// </summary>
       public ResourceSet MaterialResourceSet {get;set;}
       /// <summary>
       /// Encapsulates GPU Memory Layout and Resource. See:
       /// <see cref="Veldrid.ResourceSet"/>
       /// <see cref="Veldrid.BindableResource"/>
       /// </summary>
       public ResourceSet LightProvViewResourceSet {get;set;}
       /// <summary>
       /// Encapsulates Lighting Layout.
       /// </summary>
       public Light Light {get; set;}
       public Light SpotLight {get;set;}
       public Camera Camera {get;set;}

       public SceneRuntimeDescriptor(){
           NO_RESOURCE_SET = new ResourceSet[0];
       }

   } 
}