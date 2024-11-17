﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using RotateParticle.Components;
using UniGLTF;
using UnityEngine;
using UnityEngine.UI;


namespace UniVRM10.Cloth.Viewer
{
    public class ClothViewerUI : MonoBehaviour
    {
        [SerializeField]
        Text m_version = default;

        [Header("UI")]
        [SerializeField]
        Button m_openModel = default;

        [SerializeField]
        Button m_openMotion = default;

        [SerializeField]
        Button m_pastePose = default;

        [SerializeField]
        Button m_reconstructSprngBone = default;

        [SerializeField]
        Button m_resetSpringBone = default;

        [SerializeField]
        Toggle m_showBoxMan = default;

        [SerializeField]
        Toggle m_enableLipSync = default;

        [SerializeField]
        Toggle m_enableAutoBlink = default;

        [SerializeField]
        Toggle m_enableAutoExpression = default;

        [SerializeField]
        Toggle m_useAsync = default;

        [SerializeField]
        GameObject m_target = default;

        [SerializeField]
        TextAsset m_motion;

        GameObject Root = default;

        IVrm10Animation m_src = default;
        public IVrm10Animation Motion
        {
            get { return m_src; }
            set
            {
                if (m_src != null)
                {
                    m_src.Dispose();
                }
                m_src = value;

                TPose = new Vrm10TPose(m_src.ControlRig.Item1.GetRawHipsPosition());
            }
        }

        public IVrm10Animation TPose;

        private CancellationTokenSource _cancellationTokenSource;

        [Serializable]
        class TextFields
        {
            [SerializeField]
            Text m_textModelTitle = default;
            [SerializeField]
            Text m_textModelVersion = default;
            [SerializeField]
            Text m_textModelAuthor = default;
            [SerializeField]
            Text m_textModelCopyright = default;
            [SerializeField]
            Text m_textModelContact = default;
            [SerializeField]
            Text m_textModelReference = default;
            [SerializeField]
            RawImage m_thumbnail = default;

            [SerializeField, Header("CharacterPermission")]
            Text m_textPermissionAllowed = default;
            [SerializeField]
            Text m_textPermissionViolent = default;
            [SerializeField]
            Text m_textPermissionSexual = default;
            [SerializeField]
            Text m_textPermissionCommercial = default;
            [SerializeField]
            Text m_textPermissionOther = default;

            [SerializeField, Header("DistributionLicense")]
            Text m_textDistributionLicense = default;
            [SerializeField]
            Text m_textDistributionOther = default;

            public void Reset(ObjectMap map)
            {
                m_textModelTitle = map.Get<Text>("Title (1)");
                m_textModelVersion = map.Get<Text>("Version (1)");
                m_textModelAuthor = map.Get<Text>("Author (1)");
                m_textModelCopyright = map.Get<Text>("Copyright (1)");
                m_textModelContact = map.Get<Text>("Contact (1)");
                m_textModelReference = map.Get<Text>("Reference (1)");
                m_textPermissionAllowed = map.Get<Text>("AllowedUser (1)");
                m_textPermissionViolent = map.Get<Text>("Violent (1)");
                m_textPermissionSexual = map.Get<Text>("Sexual (1)");
                m_textPermissionCommercial = map.Get<Text>("Commercial (1)");
                m_textPermissionOther = map.Get<Text>("Other (1)");
                m_textDistributionLicense = map.Get<Text>("LicenseType (1)");
                m_textDistributionOther = map.Get<Text>("OtherLicense (1)");
                m_thumbnail = map.Get<RawImage>("RawImage");
            }

            public void Start()
            {
                m_textModelTitle.text = "";
                m_textModelVersion.text = "";
                m_textModelAuthor.text = "";
                m_textModelCopyright.text = "";
                m_textModelContact.text = "";
                m_textModelReference.text = "";

                m_textPermissionAllowed.text = "";
                m_textPermissionViolent.text = "";
                m_textPermissionSexual.text = "";
                m_textPermissionCommercial.text = "";
                m_textPermissionOther.text = "";

                m_textDistributionLicense.text = "";
                m_textDistributionOther.text = "";
            }

            public void UpdateMeta(Texture2D thumbnail, UniGLTF.Extensions.VRMC_vrm.Meta meta, Migration.Vrm0Meta meta0)
            {
                m_thumbnail.texture = thumbnail;

                if (meta != null)
                {
                    m_textModelTitle.text = meta.Name;
                    m_textModelVersion.text = meta.Version;
                    m_textModelAuthor.text = meta.Authors[0];
                    m_textModelCopyright.text = meta.CopyrightInformation;
                    m_textModelContact.text = meta.ContactInformation;
                    if (meta.References != null && meta.References.Count > 0)
                    {
                        m_textModelReference.text = meta.References[0];
                    }
                    m_textPermissionAllowed.text = meta.AvatarPermission.ToString();
                    m_textPermissionViolent.text = meta.AllowExcessivelyViolentUsage.ToString();
                    m_textPermissionSexual.text = meta.AllowExcessivelySexualUsage.ToString();
                    m_textPermissionCommercial.text = meta.CommercialUsage.ToString();
                    // m_textPermissionOther.text = meta.OtherPermissionUrl;

                    // m_textDistributionLicense.text = meta.ModificationLicense.ToString();
                    m_textDistributionOther.text = meta.OtherLicenseUrl;
                }

                if (meta0 != null)
                {
                    m_textModelTitle.text = meta0.title;
                    m_textModelVersion.text = meta0.version;
                    m_textModelAuthor.text = meta0.author;
                    m_textModelContact.text = meta0.contactInformation;
                    m_textModelReference.text = meta0.reference;
                    m_textPermissionAllowed.text = meta0.allowedUser.ToString();
                    m_textPermissionViolent.text = meta0.violentUsage.ToString();
                    m_textPermissionSexual.text = meta0.sexualUsage.ToString();
                    m_textPermissionCommercial.text = meta0.commercialUsage.ToString();
                    m_textPermissionOther.text = meta0.otherPermissionUrl;
                    // m_textDistributionLicense.text = meta0.ModificationLicense.ToString();
                    m_textDistributionOther.text = meta0.otherLicenseUrl;
                }
            }
        }
        [SerializeField]
        TextFields m_texts = default;

        [Serializable]
        class UIFields
        {
            [SerializeField]
            Toggle ToggleMotionTPose = default;

            [SerializeField]
            Toggle ToggleMotionBVH = default;

            [SerializeField]
            ToggleGroup ToggleMotion = default;

            public void Reset(ObjectMap map)
            {
                ToggleMotionTPose = map.Get<Toggle>("TPose");
                ToggleMotionBVH = map.Get<Toggle>("BVH");
                ToggleMotion = map.Get<ToggleGroup>("_Motion_");
            }

            public bool IsTPose
            {
                get => ToggleMotion.ActiveToggles().FirstOrDefault() == ToggleMotionTPose;
                set
                {
                    ToggleMotionTPose.isOn = value;
                    ToggleMotionBVH.isOn = !value;
                }
            }
        }
        [SerializeField]
        UIFields m_ui = default;

        class ObjectMap
        {
            Dictionary<string, GameObject> _map = new();
            public IReadOnlyDictionary<string, GameObject> Objects => _map;

            public ObjectMap(GameObject root)
            {
                foreach (var x in root.GetComponentsInChildren<Transform>())
                {
                    _map[x.name] = x.gameObject;
                }
            }

            public T Get<T>(string name) where T : Component
            {
                return _map[name].GetComponent<T>();
            }
        }

        private void Reset()
        {
            var map = new ObjectMap(gameObject);
            m_openModel = map.Get<Button>("OpenModel");
            m_openMotion = map.Get<Button>("OpenMotion");
            m_pastePose = map.Get<Button>("PastePose");
            m_reconstructSprngBone = map.Get<Button>("ReconstcutSpringBone");
            m_resetSpringBone = map.Get<Button>("ResetSpringBone");
            m_showBoxMan = map.Get<Toggle>("ShowBoxMan");
            m_enableLipSync = map.Get<Toggle>("EnableLipSync");
            m_enableAutoBlink = map.Get<Toggle>("EnableAutoBlink");
            m_enableAutoExpression = map.Get<Toggle>("EnableAutoExpression");
            m_useAsync = map.Get<Toggle>("UseAsync");
            m_version = map.Get<Text>("VrmVersion");
            m_texts.Reset(map);
            m_ui.Reset(map);
            m_target = GameObject.FindObjectOfType<ClothTargetMover>().gameObject;
        }

        Loaded m_loaded;
        RotateParticle.HumanoidPose m_init;
        int m_springFrame = 0;

        static class ArgumentChecker
        {
            static string[] Supported = {
                ".gltf",
                ".glb",
                ".vrm",
                ".zip",
            };

            static string UnityHubPath => System.Environment.GetEnvironmentVariable("ProgramFiles") + "\\Unity\\Hub";

            public static bool IsLoadable(string path)
            {
                if (!File.Exists(path))
                {
                    // not exists
                    return false;
                }

                if (Application.isEditor)
                {
                    // skip editor argument
                    // {UnityHub_Resources}\PackageManager\ProjectTemplates\com.unity.template.3d-5.0.4.tgz
                    if (path.StartsWith(UnityHubPath))
                    {
                        return false;
                    }
                }

                var ext = Path.GetExtension(path).ToLower();
                if (!Supported.Contains(ext))
                {
                    // unknown extension
                    return false;
                }

                return true;
            }

            public static bool TryGetFirstLoadable(out string cmd)
            {
                foreach (var arg in System.Environment.GetCommandLineArgs())
                {
                    if (ArgumentChecker.IsLoadable(arg))
                    {
                        cmd = arg;
                        return true;
                    }
                }

                cmd = default;
                return false;
            }
        }

        [SerializeField]
        public int Iteration = 32;

        Action<float> MakeSetPose()
        {
            var start = m_init;
            var animator = m_loaded.Instance.GetComponent<Animator>();
            var end = new RotateParticle.HumanoidPose(animator);
            return (float t) =>
            {
                RotateParticle.HumanoidPose.ApplyLerp(animator, start, end, t);
            };
        }

        private void Start()
        {
            m_version.text = string.Format("VRMViewer {0}.{1}",
                    VRM10SpecVersion.MAJOR, VRM10SpecVersion.MINOR);

            m_openModel.onClick.AddListener(OnOpenModelClicked);
            m_openMotion.onClick.AddListener(OnOpenMotionClicked);
            m_pastePose.onClick.AddListener(OnPastePoseClicked);
            m_reconstructSprngBone.onClick.AddListener(OnReconstruct);
            m_resetSpringBone.onClick.AddListener(OnReset);

            // load initial bvh
            if (m_motion != null)
            {
                Motion = BvhMotion.LoadBvhFromText(m_motion.text);
            }

            if (ArgumentChecker.TryGetFirstLoadable(out var cmd))
            {
                LoadModel(cmd);
            }

            m_texts.Start();
        }

        private void OnDestroy()
        {
            _cancellationTokenSource?.Dispose();
        }


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (Root != null) Root.SetActive(!Root.activeSelf);
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (_cancellationTokenSource != null)
                {
                    _cancellationTokenSource.Cancel();
                }
            }

            if (Motion != null)
            {
                Motion.ShowBoxMan(m_showBoxMan.isOn);
            }

            if (m_loaded != null)
            {
                m_loaded.EnableLipSyncValue = m_enableLipSync.isOn;
                m_loaded.EnableBlinkValue = m_enableAutoBlink.isOn;
                m_loaded.EnableAutoExpressionValue = m_enableAutoExpression.isOn;
            }

            if (m_loaded != null)
            {
                if (m_ui.IsTPose)
                {
                    m_loaded.Runtime.VrmAnimation = TPose;
                }
                else if (Motion != null)
                {
                    // Automatically retarget in Vrm10Runtime.Process
                    m_loaded.Runtime.VrmAnimation = Motion;
                }
            }
        }

        void OnOpenModelClicked()
        {
#if UNITY_EDITOR
            var path = UnityEditor.EditorUtility.OpenFilePanel("Open VRM", "", "vrm");
#elif UNITY_STANDALONE_WIN
            var path = ClothFileDialogForWindows.FileDialog("open VRM", "vrm");
#else
            var path = Application.dataPath + "/default.vrm";
#endif
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var ext = Path.GetExtension(path).ToLower();
            if (ext != ".vrm")
            {
                Debug.LogWarning($"{path} is not vrm");
                return;
            }

            LoadModel(path);
        }

        async void OnOpenMotionClicked()
        {
#if UNITY_STANDALONE_WIN
            var path = ClothFileDialogForWindows.FileDialog("open Motion", "bvh", "gltf", "glb", "vrma");
#elif UNITY_EDITOR
            var path = UnityEditor.EditorUtility.OpenFilePanel("Open Motion", "", "bvh");
#else
            var path = Application.dataPath + "/default.bvh";
#endif
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var ext = Path.GetExtension(path).ToLower();
            if (ext == ".bvh")
            {
                Motion = BvhMotion.LoadBvhFromPath(path);
                return;
            }

            // gltf, glb etc...
            using GltfData data = new AutoGltfFileParser(path).Parse();
            using var loader = new VrmAnimationImporter(data);
            var instance = await loader.LoadAsync(new ImmediateCaller());
            Motion = instance.GetComponent<Vrm10AnimationInstance>();
            instance.GetComponent<Animation>().Play();
        }

        async void OnPastePoseClicked()
        {
            var text = GUIUtility.systemCopyBuffer;
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            try
            {
                Motion = await Vrm10PoseLoader.LoadVrmAnimationPose(text);
            }
            catch (UniJSON.ParserException)
            {
                Debug.LogWarning("UniJSON.ParserException");
            }
            catch (UniJSON.DeserializationException)
            {
                Debug.LogWarning("UniJSON.DeserializationException");
            }
        }

        void OnReconstruct()
        {
            if (m_loaded == null)
            {
                return;
            }
            m_loaded.Runtime.SpringBone.ReconstructSpringBone();
            // var system = m_loaded.Instance.GetComponent<RotateParticle.RotateParticleSystem>();
            // system.ResetParticle();
        }

        void OnReset()
        {
            if (m_loaded == null)
            {
                return;
            }
            m_loaded.Runtime.SpringBone.RestoreInitialTransform();
            // ResetStrandPose();
        }

        // void ResetStrandPose()
        // {
        //     ResetStrandPose(MakeSetPose(), 32, 1.0f / 30, 60);
        // }

        // void ResetStrandPose(Action<float> setPose, int iteration, float timeDelta, int finish)
        // {
        //     var system = m_loaded.Instance.GetComponent<RotateParticle.RotateParticleSystem>();

        //     // init
        //     setPose(0);
        //     system.ResetParticle();

        //     // lerp
        //     var t = 0.0f;
        //     var d = 1.0f / iteration;
        //     for (int i = 0; i < iteration; ++i, t += d)
        //     {
        //         setPose(t);
        //         system.Process(timeDelta);
        //     }

        //     // finish
        //     setPose(1.0f);
        //     for (int i = 0; i < finish; ++i)
        //     {
        //         system.Process(timeDelta);
        //     }
        // }

        static IMaterialDescriptorGenerator GetVrmMaterialDescriptorGenerator(bool useUrp)
        {
            if (useUrp)
            {
                return new UrpVrm10MaterialDescriptorGenerator();
            }
            else
            {
                return new BuiltInVrm10MaterialDescriptorGenerator();
            }
        }

        void OnInit(Vrm10Instance vrm)
        {
            var animator = vrm.GetComponent<Animator>();

            try
            {
                if (vrm.SpringBone.Springs.Count == 0)
                {
                    ClothGuess.Guess(animator);
                    if (vrm.SpringBone.ColliderGroups.Count == 0)
                    {
                        HumanoidCollider.AddColliders(animator);
                        var warps = animator.GetComponentsInChildren<Warp>();
                        var colliderGroups = animator.GetComponentsInChildren<VRM10SpringBoneColliderGroup>();
                        foreach (var warp in warps)
                        {
                            warp.ColliderGroups = colliderGroups.ToList();
                        }
                    }
                }
                else
                {
                    RotateParticleRuntimeProvider.FromVrm10(vrm,
                        go => go.AddComponent<Warp>(),
                        o => GameObject.DestroyImmediate(o));
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        async void LoadModel(string path)
        {
            // cleanup
            m_loaded?.Dispose();
            m_loaded = null;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            try
            {
                Debug.LogFormat("{0}", path);
                var vrm10Instance = await Vrm10.LoadPathAsync(path,
                    canLoadVrm0X: true,
                    showMeshes: false,
                    awaitCaller: m_useAsync.enabled ? (IAwaitCaller)new RuntimeOnlyAwaitCaller() : (IAwaitCaller)new ImmediateCaller(),
                    materialGenerator: GetVrmMaterialDescriptorGenerator(true),
                    vrmMetaInformationCallback: m_texts.UpdateMeta,
                    springboneRuntime: new RotateParticle.RotateParticleSpringboneRuntime(OnInit));
                if (cancellationToken.IsCancellationRequested)
                {
                    UnityObjectDestroyer.DestroyRuntimeOrEditor(vrm10Instance.gameObject);
                    cancellationToken.ThrowIfCancellationRequested();
                }

                if (vrm10Instance == null)
                {
                    Debug.LogWarning("LoadPathAsync is null");
                    return;
                }

                var instance = vrm10Instance.GetComponent<RuntimeGltfInstance>();
                instance.ShowMeshes();
                instance.EnableUpdateWhenOffscreen();
                m_loaded = new Loaded(instance, m_target.transform);
                m_init = new RotateParticle.HumanoidPose(vrm10Instance.GetComponent<Animator>());
                m_springFrame = 0;
                m_showBoxMan.isOn = false;
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException)
                {
                    Debug.LogWarning($"Canceled to Load: {path}");
                }
                else
                {
                    Debug.LogError($"Failed to Load: {path}");
                    Debug.LogException(ex);
                }
            }
        }
    }
}
