using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace UniGLTF.MeshUtility
{
    /// <summary>
    /// - Freeze
    /// - Integration
    /// - Split
    /// 
    /// - Implement runtime logic => Process a hierarchy in scene. Do not process prefab.
    /// - Implement undo
    ///
    /// </summary>
    public class GltfMeshUtility
    {
        /// <summary>
        /// GameObject 名が重複している場合にリネームする。
        /// 最初に実行(Avatar生成時のエラーを回避？)
        /// </summary>
        public bool ForceUniqueName = false;

        /// <summary>
        /// Same as VRM-0 normalization
        /// - Mesh
        /// - Node
        /// - InverseBindMatrices
        /// </summary>
        public bool FreezeBlendShape = false;

        /// <summary>
        /// Same as VRM-0 normalization
        /// - Mesh
        /// - Node
        /// - InverseBindMatrices
        /// </summary>
        public bool FreezeScaling = true;

        /// <summary>
        /// Same as VRM-0 normalization
        /// - Mesh
        /// - Node
        /// - InverseBindMatrices
        /// </summary>
        public bool FreezeRotation = false;

        public List<MeshIntegrationGroup> MeshIntegrationGroups = new List<MeshIntegrationGroup>();

        /// <summary>
        /// Create a headless model and solve VRM.FirstPersonFlag.Auto
        /// </summary>
        public bool GenerateMeshForFirstPersonAuto = false;

        /// <summary>
        /// Split into having and not having BlendShape
        /// </summary>
        public bool SplitByBlendShape = false;

        public void IntegrateAll(GameObject root)
        {
            if (root == null)
            {
                return;
            }
            MeshIntegrationGroups.Add(new MeshIntegrationGroup
            {
                Name = "ALL",
                Renderers = root.GetComponentsInChildren<Renderer>().ToList(),
            });
        }

        void RemoveComponent<T>(T c) where T : Component
        {
            if (c == null)
            {
                return;
            }
            var t = c.transform;
            if (Application.isPlaying)
            {
                GameObject.Destroy(c);
            }
            else
            {
                GameObject.DestroyImmediate(c);
            }

            if (t.childCount == 0)
            {
                var list = t.GetComponents<Component>();
                // Debug.Log($"{list[0].GetType().Name}");
                if (list.Length == 1 && list[0] == t)
                {
                    if (Application.isPlaying)
                    {
                        GameObject.Destroy(t.gameObject);
                    }
                    else
                    {
                        GameObject.DestroyImmediate(t.gameObject);
                    }
                }
            }
        }

        static GameObject GetOrCreateEmpty(GameObject go, string name)
        {
            foreach (var child in go.transform.GetChildren())
            {
                if (child.name == name
                 && child.localPosition == Vector3.zero
                 && child.localScale == Vector3.one
                 && child.localRotation == Quaternion.identity)
                {
                    return child.gameObject;
                }
            }
            var empty = new GameObject(name);
            empty.transform.SetParent(go.transform, false);
            return empty;
        }

        protected virtual List<MeshIntegrationGroup> CopyMeshIntegrationGroups()
        {
            return MeshIntegrationGroups.ToList();
        }

        public virtual List<GameObject> Process(GameObject go)
        {
            // TODO unpack prefab

            // 正規化されたヒエラルキーを作る
            if (FreezeBlendShape || FreezeRotation || FreezeScaling)
            {
                var (normalized, boneMap) = BoneNormalizer.NormalizeHierarchyFreezeMesh(go,
                    removeScaling: FreezeScaling,
                    removeRotation: FreezeRotation,
                    freezeBlendShape: FreezeBlendShape);

                // write back normalized transform to boneMap
                BoneNormalizer.WriteBackResult(go, normalized, boneMap);
                if (Application.isPlaying)
                {
                    GameObject.Destroy(normalized);
                }
                else
                {
                    GameObject.DestroyImmediate(normalized);
                }
            }

            var copy = CopyMeshIntegrationGroups();

            var newGo = new List<GameObject>();
            {
                var empty = GetOrCreateEmpty(go, "mesh");

                var results = new List<MeshIntegrationResult>();
                foreach (var group in copy)
                {
                    Integrate(newGo, empty, results, group);
                }

                foreach (var result in results)
                {
                    foreach (var r in result.SourceMeshRenderers)
                    {
                        RemoveComponent(r);
                    }
                    foreach (var r in result.SourceSkinnedMeshRenderers)
                    {
                        RemoveComponent(r);
                    }
                }
            }

            MeshIntegrationGroups.Clear();

            return newGo;
        }

        protected virtual MeshIntegrationResult Integrate(List<GameObject> newGo,
            GameObject empty, List<MeshIntegrationResult> results, MeshIntegrationGroup group)
        {
            var result = MeshIntegrator.Integrate(group, SplitByBlendShape
                ? MeshIntegrator.BlendShapeOperation.Split
                : MeshIntegrator.BlendShapeOperation.Use);
            results.Add(result);

            foreach (var created in result.AddIntegratedRendererTo(empty))
            {
                newGo.Add(created);
            }

            return result;
        }
    }
}
