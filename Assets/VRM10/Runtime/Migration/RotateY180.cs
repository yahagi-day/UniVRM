﻿using System;
using System.Collections.Generic;
using UniGLTF;

namespace UniVRM10
{
    /// <summary>
    /// x, y, z => -x, y, -z
    /// </summary>
    public static class RotateY180
    {
        public static void Rotate(glTFNode node)
        {
            if (node.matrix != null && node.matrix.Length == 16)
            {
                throw new NotImplementedException("matrix not implemented !");
            }
            else
            {
                if (node.translation != null && node.translation.Length == 3)
                {
                    var t = node.translation;
                    t[0] = -t[0];
                    t[2] = -t[2];
                }
                if (node.rotation != null && node.rotation.Length == 4)
                {
                    // throw new NotImplementedException("not normlaized !");
                }
                if (node.scale != null && node.rotation.Length == 3)
                {
                    // do nothing
                }
            }
        }

        static void ReverseVector3Array(glTF gltf, int accessorIndex, HashSet<int> used)
        {
            if (!used.Add(accessorIndex))
            {
                return;
            }

            var accessor = gltf.accessors[accessorIndex];
            var buffer = gltf.GetViewBytes(accessor.bufferView);
            var span = VrmLib.SpanLike.Wrap<UnityEngine.Vector3>(buffer);
            for (int i = 0; i < span.Length; ++i)
            {
                span[i] = span[i].RotateY180();
            }
        }

        /// <summary>
        /// シーンをY軸で180度回転する
        /// </summary>
        /// <param name="gltf"></param>
        public static void Rotate(glTF gltf)
        {
            // foreach (var node in gltf.nodes)
            // {
            //     Rotate(node);
            // }

            // mesh の回転のみでよい
            var used = new HashSet<int>();
            foreach (var mesh in gltf.meshes)
            {
                foreach (var prim in mesh.primitives)
                {
                    ReverseVector3Array(gltf, prim.attributes.POSITION, used);
                    ReverseVector3Array(gltf, prim.attributes.NORMAL, used);
                    foreach (var target in prim.targets)
                    {
                        ReverseVector3Array(gltf, target.POSITION, used);
                        ReverseVector3Array(gltf, target.NORMAL, used);
                    }
                }
            }

            // foreach (var skin in gltf.skins)
            // {
            //     if (!used.Add(skin.inverseBindMatrices))
            //     {
            //         var accessor = gltf.accessors[skin.inverseBindMatrices];
            //         var buffer = gltf.GetViewBytes(accessor.bufferView);
            //         var span = VrmLib.SpanLike.Wrap<UnityEngine.Matrix4x4>(buffer);
            //         for (int i = 0; i < span.Length; ++i)
            //         {
            //             span[i] = span[i].RotateY180();
            //         }
            //     }
            // }
        }
    }
}
