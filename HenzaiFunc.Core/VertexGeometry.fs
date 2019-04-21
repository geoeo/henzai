namespace HenzaiFunc.Core

open System.Numerics
open Henzai.Core.VertexGeometry
open Henzai.Core.Numerics


module VertexGeometry = 
    
    let vertexPositionTransform(transform : Matrix4x4) (v : VertexPosition) = 
        let transformedVector4 = Vector4.Transform(v.GetPosition(), transform)
        VertexPosition(transformedVector4)

    let vertexPositionNormalTransform(transform : Matrix4x4) (v : VertexPositionNormal) = 
        let transformedVector4 = Vector4.Transform(v.GetPosition(), transform)
        let rot = Geometry.Rotation(ref transform)
        let transformedNormal = Vector4.Transform(v.GetNormal(), rot)
        VertexPositionNormal(Vector.ToVec3(transformedVector4), Vector.ToVec3(transformedNormal))

    let vertexPositionColorTransform(transform : Matrix4x4) (v : VertexPositionColor) = 
        let transformedVector4 = Vector4.Transform(v.GetPosition(), transform)
        VertexPositionColor(transformedVector4, v)

    let vertexPositionNDCColorTransform(transform : Matrix4x4) (v : VertexPositionNDCColor) = 
        let transformedVector4 = Vector4.Transform(v.GetPosition(), transform)
        VertexPositionNDCColor(transformedVector4, v)

    //TODO: Transform Normal for all of these
    // let vertexPositionNormalTexture(transform : Matrix4x4) (v : VertexPositionNormalTexture) = 
    //     let transformedVector4 = Vector4.Transform(v.GetPosition(), transform)
    //     VertexPositionNormalTexture(transformedVector4, v)

    // let vertexPositionNormalTextureTangentTransform(transform : Matrix4x4) (v : VertexPositionNormalTextureTangent) = 
    //     let transformedVector4 = Vector4.Transform(v.GetPosition(), transform)
    //     VertexPositionNormalTextureTangent(transformedVector4, v)

    // let vertexPositionNormalTextureTangentBitangentTransform(transform : Matrix4x4) (v : VertexPositionNormalTextureTangentBitangent) = 
    //     let transformedVector4 = Vector4.Transform(v.GetPosition(), transform)
    //     VertexPositionNormalTextureTangentBitangent(transformedVector4, v)

    // let vertexPositionTextureTransform(transform : Matrix4x4) (v : VertexPositionTexture) = 
    //     let transformedVector4 = Vector4.Transform(v.GetPosition(), transform)
    //     VertexPositionTexture(transformedVector4, v)

    