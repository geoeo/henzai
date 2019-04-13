namespace HenzaiFunc.Core

open System.Numerics
open Henzai.Core.VertexGeometry



module VertexGeometry = 
    
    let vertexPositionTransform(transform : Matrix4x4) (v : VertexPosition) = 
        let transformedVector4 = Vector4.Transform(v.GetPosition(), transform)
        VertexPosition(transformedVector4)

    let vertexPositionNormalTransform(transform : Matrix4x4) (v : VertexPositionNormal) = 
        let transformedVector4 = Vector4.Transform(v.GetPosition(), transform)
        VertexPositionNormal(transformedVector4, v)

    let vertexPositionColorTransform(transform : Matrix4x4) (v : VertexPositionColor) = 
        let transformedVector4 = Vector4.Transform(v.GetPosition(), transform)
        VertexPositionColor(transformedVector4, v)

    let vertexPositionNDCColorTransform(transform : Matrix4x4) (v : VertexPositionNDCColor) = 
        let transformedVector4 = Vector4.Transform(v.GetPosition(), transform)
        VertexPositionNDCColor(transformedVector4, v)

    let vertexPositionNormalTexture(transform : Matrix4x4) (v : VertexPositionNormalTexture) = 
        let transformedVector4 = Vector4.Transform(v.GetPosition(), transform)
        VertexPositionNormalTexture(transformedVector4, v)

    let vertexPositionNormalTextureTangentTransform(transform : Matrix4x4) (v : VertexPositionNormalTextureTangent) = 
        let transformedVector4 = Vector4.Transform(v.GetPosition(), transform)
        VertexPositionNormalTextureTangent(transformedVector4, v)

    let vertexPositionNormalTextureTangentBitangentTransform(transform : Matrix4x4) (v : VertexPositionNormalTextureTangentBitangent) = 
        let transformedVector4 = Vector4.Transform(v.GetPosition(), transform)
        VertexPositionNormalTextureTangentBitangent(transformedVector4, v)

    let vertexPositionTextureTransform(transform : Matrix4x4) (v : VertexPositionTexture) = 
        let transformedVector4 = Vector4.Transform(v.GetPosition(), transform)
        VertexPositionTexture(transformedVector4, v)