using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;
using UnityEngine.U2D;

namespace Better.BuildInfo.Internal
{
    /// <summary>
    /// <see>https://docs.unity3d.com/Manual/ClassIDReference.html</see>
    /// </summary>
    internal enum YamlClassId
    {
        Unknown = 0,
        GameObject = 1,
        Component = 2,
        LevelGameManager = 3,
        Transform = 4,
        TimeManager = 5,
        GlobalGameManager = 6,
        Behaviour = 8,
        GameManager = 9,
        AudioManager = 11,
        ParticleAnimator = 12,
        InputManager = 13,
        EllipsoidParticleEmitter = 15,
        Pipeline = 17,
        EditorExtension = 18,
        Physics2DSettings = 19,
        Camera = 20,
        Material = 21,
        MeshRenderer = 23,
        Renderer = 25,
        ParticleRenderer = 26,
        Texture = 27,
        Texture2D = 28,
        SceneSettings = 29,
        GraphicsSettings = 30,
        MeshFilter = 33,
        OcclusionPortal = 41,
        Mesh = 43,
        Skybox = 45,
        QualitySettings = 47,
        Shader = 48,
        TextAsset = 49,
        Rigidbody2D = 50,
        Physics2DManager = 51,
        Collider2D = 53,
        Rigidbody = 54,
        PhysicsManager = 55,
        Collider = 56,
        Joint = 57,
        CircleCollider2D = 58,
        HingeJoint = 59,
        PolygonCollider2D = 60,
        BoxCollider2D = 61,
        PhysicsMaterial2D = 62,
        MeshCollider = 64,
        BoxCollider = 65,
        SpriteCollider2D = 66,
        EdgeCollider2D = 68,
        ComputeShader = 72,
        AnimationClip = 74,
        ConstantForce = 75,
        WorldParticleCollider = 76,
        TagManager = 78,
        AudioListener = 81,
        AudioSource = 82,
        AudioClip = 83,
        RenderTexture = 84,
        MeshParticleEmitter = 87,
        ParticleEmitter = 88,
        Cubemap = 89,
        Avatar = 90,
        AnimatorController = 91,
        GUILayer = 92,
        RuntimeAnimatorController = 93,
        ScriptMapper = 94,
        Animator = 95,
        TrailRenderer = 96,
        DelayedCallManager = 98,
        TextMesh = 102,
        RenderSettings = 104,
        Light = 108,
        CGProgram = 109,
        BaseAnimationTrack = 110,
        Animation = 111,
        MonoBehaviour = 114,
        MonoScript = 115,
        MonoManager = 116,
        Texture3D = 117,
        NewAnimationTrack = 118,
        Projector = 119,
        LineRenderer = 120,
        Flare = 121,
        Halo = 122,
        LensFlare = 123,
        FlareLayer = 124,
        HaloLayer = 125,
        NavMeshAreas = 126,
        HaloManager = 127,
        Font = 128,
        PlayerSettings = 129,
        NamedObject = 130,
        GUITexture = 131,
        GUIText = 132,
        GUIElement = 133,
        PhysicMaterial = 134,
        SphereCollider = 135,
        CapsuleCollider = 136,
        SkinnedMeshRenderer = 137,
        FixedJoint = 138,
        RaycastCollider = 140,
        BuildSettings = 141,
        AssetBundle = 142,
        CharacterController = 143,
        CharacterJoint = 144,
        SpringJoint = 145,
        WheelCollider = 146,
        ResourceManager = 147,
        NetworkView = 148,
        NetworkManager = 149,
        PreloadData = 150,
        MovieTexture = 152,
        ConfigurableJoint = 153,
        TerrainCollider = 154,
        MasterServerInterface = 155,
        TerrainData = 156,
        LightmapSettings = 157,
        WebCamTexture = 158,
        EditorSettings = 159,
        InteractiveCloth = 160,
        ClothRenderer = 161,
        EditorUserSettings = 162,
        SkinnedCloth = 163,
        AudioReverbFilter = 164,
        AudioHighPassFilter = 165,
        AudioChorusFilter = 166,
        AudioReverbZone = 167,
        AudioEchoFilter = 168,
        AudioLowPassFilter = 169,
        AudioDistortionFilter = 170,
        SparseTexture = 171,
        AudioBehaviour = 180,
        AudioFilter = 181,
        WindZone = 182,
        Cloth = 183,
        SubstanceArchive = 184,
        ProceduralMaterial = 185,
        ProceduralTexture = 186,
        OffMeshLink = 191,
        OcclusionArea = 192,
        Tree = 193,
        NavMeshObsolete = 194,
        NavMeshAgent = 195,
        NavMeshSettings = 196,
        LightProbesLegacy = 197,
        ParticleSystem = 198,
        ParticleSystemRenderer = 199,
        ShaderVariantCollection = 200,
        LODGroup = 205,
        BlendTree = 206,
        Motion = 207,
        NavMeshObstacle = 208,
        TerrainInstance = 210,
        SpriteRenderer = 212,
        Sprite = 213,
        CachedSpriteAtlas = 214,
        ReflectionProbe = 215,
        ReflectionProbes = 216,
        Terrain = 218,
        LightProbeGroup = 220,
        AnimatorOverrideController = 221,
        CanvasRenderer = 222,
        Canvas = 223,
        RectTransform = 224,
        CanvasGroup = 225,
        BillboardAsset = 226,
        BillboardRenderer = 227,
        SpeedTreeWindAsset = 228,
        AnchoredJoint2D = 229,
        Joint2D = 230,
        SpringJoint2D = 231,
        DistanceJoint2D = 232,
        HingeJoint2D = 233,
        SliderJoint2D = 234,
        WheelJoint2D = 235,
        NavMeshData = 238,
        AudioMixer = 240,
        AudioMixerController = 241,
        AudioMixerGroupController = 243,
        AudioMixerEffectController = 244,
        AudioMixerSnapshotController = 245,
        PhysicsUpdateBehaviour2D = 246,
        ConstantForce2D = 247,
        Effector2D = 248,
        AreaEffector2D = 249,
        PointEffector2D = 250,
        PlatformEffector2D = 251,
        SurfaceEffector2D = 252,
        LightProbes = 258,
        SampleClip = 271,
        AudioMixerSnapshot = 272,
        AudioMixerGroup = 273,
        AssetBundleManifest = 290,
        Prefab = 1001,
        EditorExtensionImpl = 1002,
        AssetImporter = 1003,
        AssetDatabase = 1004,
        Mesh3DSImporter = 1005,
        TextureImporter = 1006,
        ShaderImporter = 1007,
        ComputeShaderImporter = 1008,
        AvatarMask = 1011,
        AudioImporter = 1020,
        HierarchyState = 1026,
        GUIDSerializer = 1027,
        AssetMetaData = 1028,
        DefaultAsset = 1029,
        DefaultImporter = 1030,
        TextScriptImporter = 1031,
        SceneAsset = 1032,
        NativeFormatImporter = 1034,
        MonoImporter = 1035,
        AssetServerCache = 1037,
        LibraryAssetImporter = 1038,
        ModelImporter = 1040,
        FBXImporter = 1041,
        TrueTypeFontImporter = 1042,
        MovieImporter = 1044,
        EditorBuildSettings = 1045,
        DDSImporter = 1046,
        InspectorExpandedState = 1048,
        AnnotationManager = 1049,
        PluginImporter = 1050,
        EditorUserBuildSettings = 1051,
        PVRImporter = 1052,
        ASTCImporter = 1053,
        KTXImporter = 1054,
        AnimatorStateTransition = 1101,
        AnimatorState = 1102,
        HumanTemplate = 1105,
        AnimatorStateMachine = 1107,
        PreviewAssetType = 1108,
        AnimatorTransition = 1109,
        SpeedTreeImporter = 1110,
        AnimatorTransitionBase = 1111,
        SubstanceImporter = 1112,
        LightmapParameters = 1113,
        LightmapSnapshot = 1120,
        SketchUpImporter = 1124,
        BuildReport = 1125,
        PackedAssets = 1126,
        VideoClipImporter = 1127, 
        // int = 100000, 
        // bool = 100001, 
        // float = 100002,
        MonoObject = 100003,
        Collision = 100004,
        Vector3f = 100005,
        RootMotionData = 100006,
        Collision2D = 100007,
        AudioMixerLiveUpdateFloat = 100008,
        AudioMixerLiveUpdateBool = 100009,
        Polygon2D = 100010, 
        // void = 100011,
        TilemapCollider2D = 19719996,
        AssetImporterLog = 41386430,
        VFXRenderer = 73398921,
        SerializableManagedRefTestClass = 76251197,
        Grid = 156049354,
        Preset = 181963792,
        EmptyObject = 277625683,
        IConstraint = 285090594,
        TestObjectWithSpecialLayoutOne = 293259124,
        AssemblyDefinitionReferenceImporter = 294290339,
        SiblingDerived = 334799969,
        TestObjectWithSerializedMapStringNonAlignedStruct = 342846651,
        SubDerived = 367388927,
        AssetImportInProgressProxy = 369655926,
        PluginBuildInfo = 382020655,
        EditorProjectAccess = 426301858,
        PrefabImporter = 468431735,
        TestObjectWithSerializedArray = 478637458,
        TestObjectWithSerializedAnimationCurve = 478637459,
        TilemapRenderer = 483693784,
        SpriteAtlasDatabase = 638013454,
        AudioBuildInfo = 641289076,
        CachedSpriteAtlasRuntimeData = 644342135,
        RendererFake = 646504946,
        AssemblyDefinitionReferenceAsset = 662584278,
        BuiltAssetBundleInfoSet = 668709126,
        SpriteAtlas = 687078895,
        RayTracingShaderImporter = 747330370,
        RayTracingShader = 825902497,
        PlatformModuleSetup = 877146078,
        AimConstraint = 895512359,
        VFXManager = 937362698,
        VisualEffectSubgraph = 994735392,
        VisualEffectSubgraphOperator = 994735403,
        VisualEffectSubgraphBlock = 994735404,
        // Prefab = 1001480554,
        LocalizationImporter = 1027052791,
        Derived = 1091556383,
        PropertyModificationsTargetTestObject = 1111377672,
        ReferencesArtifactGenerator = 1114811875,
        AssemblyDefinitionAsset = 1152215463,
        SceneVisibilityState = 1154873562,
        LookAtConstraint = 1183024399,
        MultiArtifactTestImporter = 1223240404,
        GameObjectRecorder = 1268269756,
        LightingDataAssetParent = 1325145578,
        PresetManager = 1386491679,
        TestObjectWithSpecialLayoutTwo = 1392443030,
        StreamingManager = 1403656975,
        LowerResBlitTexture = 1480428607,
        StreamingController = 1542919678,
        TestObjectVectorPairStringBool = 1628831178,
        GridLayout = 1742807556,
        AssemblyDefinitionImporter = 1766753193,
        ParentConstraint = 1773428102,
        FakeComponent = 1803986026,
        PositionConstraint = 1818360608,
        RotationConstraint = 1818360609,
        ScaleConstraint = 1818360610,
        Tilemap = 1839735485,
        PackageManifest = 1896753125,
        PackageManifestImporter = 1896753126,
        TerrainLayer = 1953259897,
        SpriteShapeRenderer = 1971053207,
        NativeObjectType = 1977754360,
        TestObjectWithSerializedMapStringBool = 1981279845,
        SerializableManagedHost = 1995898324,
        VisualEffectAsset = 2058629509,
        VisualEffectImporter = 2058629510,
        VisualEffectResource = 2058629511,
        VisualEffectObject = 2059678085,
        VisualEffect = 2083052967,
        LocalizationAsset = 2083778819,
        ScriptedImporter = 2089858483,
    }

    internal static class YamlClassIdExtensions
    {
        private static System.Collections.BitArray Components;

        public static bool IsComponent(this YamlClassId val) => (int)val < 253 && Components[(int)val];
        public static bool IsTransform(this YamlClassId val) => val == YamlClassId.Transform || val == YamlClassId.RectTransform;

        static YamlClassIdExtensions()
        {
            Components = new System.Collections.BitArray(253);
            Components[(int)YamlClassId.Component] = true;
            Components[(int)YamlClassId.Transform] = true;
            Components[(int)YamlClassId.Behaviour] = true;
            Components[(int)YamlClassId.Camera] = true;
            Components[(int)YamlClassId.MeshRenderer] = true;
            Components[(int)YamlClassId.Renderer] = true;
            Components[(int)YamlClassId.MeshFilter] = true;
            Components[(int)YamlClassId.Rigidbody2D] = true;
            Components[(int)YamlClassId.Collider2D] = true;
            Components[(int)YamlClassId.Rigidbody] = true;
            Components[(int)YamlClassId.Collider] = true;
            Components[(int)YamlClassId.Joint] = true;
            Components[(int)YamlClassId.CircleCollider2D] = true;
            Components[(int)YamlClassId.HingeJoint] = true;
            Components[(int)YamlClassId.PolygonCollider2D] = true;
            Components[(int)YamlClassId.BoxCollider2D] = true;
            Components[(int)YamlClassId.MeshCollider] = true;
            Components[(int)YamlClassId.BoxCollider] = true;
            Components[(int)YamlClassId.EdgeCollider2D] = true;
            Components[(int)YamlClassId.AudioListener] = true;
            Components[(int)YamlClassId.AudioSource] = true;
            Components[(int)YamlClassId.Animator] = true;
            Components[(int)YamlClassId.TrailRenderer] = true;
            Components[(int)YamlClassId.TextMesh] = true;
            Components[(int)YamlClassId.Light] = true;
            Components[(int)YamlClassId.Animation] = true;
            Components[(int)YamlClassId.MonoBehaviour] = true;
            Components[(int)YamlClassId.Projector] = true;
            Components[(int)YamlClassId.LineRenderer] = true;
            Components[(int)YamlClassId.LensFlare] = true;
            Components[(int)YamlClassId.FlareLayer] = true;
            Components[(int)YamlClassId.SphereCollider] = true;
            Components[(int)YamlClassId.CapsuleCollider] = true;
            Components[(int)YamlClassId.SkinnedMeshRenderer] = true;
            Components[(int)YamlClassId.FixedJoint] = true;
            Components[(int)YamlClassId.CharacterController] = true;
            Components[(int)YamlClassId.CharacterJoint] = true;
            Components[(int)YamlClassId.SpringJoint] = true;
            Components[(int)YamlClassId.WheelCollider] = true;
            Components[(int)YamlClassId.ConfigurableJoint] = true;
            Components[(int)YamlClassId.TerrainCollider] = true;
            Components[(int)YamlClassId.AudioReverbFilter] = true;
            Components[(int)YamlClassId.AudioHighPassFilter] = true;
            Components[(int)YamlClassId.AudioChorusFilter] = true;
            Components[(int)YamlClassId.AudioReverbZone] = true;
            Components[(int)YamlClassId.AudioEchoFilter] = true;
            Components[(int)YamlClassId.AudioLowPassFilter] = true;
            Components[(int)YamlClassId.AudioDistortionFilter] = true;
            Components[(int)YamlClassId.AudioBehaviour] = true;
            Components[(int)YamlClassId.WindZone] = true;
            Components[(int)YamlClassId.Cloth] = true;
            Components[(int)YamlClassId.OcclusionArea] = true;
            Components[(int)YamlClassId.Tree] = true;
            Components[(int)YamlClassId.ParticleSystem] = true;
            Components[(int)YamlClassId.ParticleSystemRenderer] = true;
            Components[(int)YamlClassId.LODGroup] = true;
            Components[(int)YamlClassId.SpriteRenderer] = true;
            Components[(int)YamlClassId.ReflectionProbe] = true;
            Components[(int)YamlClassId.Terrain] = true;
            Components[(int)YamlClassId.LightProbeGroup] = true;
            Components[(int)YamlClassId.CanvasRenderer] = true;
            Components[(int)YamlClassId.Canvas] = true;
            Components[(int)YamlClassId.RectTransform] = true;
            Components[(int)YamlClassId.CanvasGroup] = true;
            Components[(int)YamlClassId.BillboardRenderer] = true;
            Components[(int)YamlClassId.AnchoredJoint2D] = true;
            Components[(int)YamlClassId.Joint2D] = true;
            Components[(int)YamlClassId.SpringJoint2D] = true;
            Components[(int)YamlClassId.DistanceJoint2D] = true;
            Components[(int)YamlClassId.HingeJoint2D] = true;
            Components[(int)YamlClassId.SliderJoint2D] = true;
            Components[(int)YamlClassId.WheelJoint2D] = true;
            Components[(int)YamlClassId.PhysicsUpdateBehaviour2D] = true;
            Components[(int)YamlClassId.ConstantForce2D] = true;
            Components[(int)YamlClassId.Effector2D] = true;
            Components[(int)YamlClassId.AreaEffector2D] = true;
            Components[(int)YamlClassId.PointEffector2D] = true;
            Components[(int)YamlClassId.PlatformEffector2D] = true;
            Components[(int)YamlClassId.SurfaceEffector2D] = true;
        }

#if UNITY_2017_1_OR_NEWER
        public static Type ToType(this YamlClassId value)
        {
            switch (value)
            {
                case YamlClassId.GameObject: return typeof(GameObject);
                case YamlClassId.Component: return typeof(Component);
                //case YamlClassId.LevelGameManager: return typeof(LevelGameManager);
                case YamlClassId.Transform: return typeof(Transform);
                //case YamlClassId.TimeManager: return typeof(TimeManager);
                //case YamlClassId.GlobalGameManager: return typeof(GlobalGameManager);
                case YamlClassId.Behaviour: return typeof(Behaviour);
                //case YamlClassId.GameManager: return typeof(GameManager);
                //case YamlClassId.AudioManager: return typeof(AudioManager);
                //case YamlClassId.ParticleAnimator: return typeof(ParticleAnimator);
                //case YamlClassId.InputManager: return typeof(InputManager);
                //case YamlClassId.EllipsoidParticleEmitter: return typeof(EllipsoidParticleEmitter);
                //case YamlClassId.Pipeline: return typeof(Pipeline);
                //case YamlClassId.EditorExtension: return typeof(EditorExtension);
                //case YamlClassId.Physics2DSettings: return typeof(Physics2DSettings);
                case YamlClassId.Camera: return typeof(Camera);
                case YamlClassId.Material: return typeof(Material);
                case YamlClassId.MeshRenderer: return typeof(MeshRenderer);
                case YamlClassId.Renderer: return typeof(Renderer);
                //case YamlClassId.ParticleRenderer: return typeof(ParticleRenderer);
                case YamlClassId.Texture: return typeof(Texture);
                case YamlClassId.Texture2D: return typeof(Texture2D);
                //case YamlClassId.SceneSettings: return typeof(SceneSettings);
                //case YamlClassId.GraphicsSettings: return typeof(GraphicsSettings);
                case YamlClassId.MeshFilter: return typeof(MeshFilter);
                case YamlClassId.OcclusionPortal: return typeof(OcclusionPortal);
                case YamlClassId.Mesh: return typeof(Mesh);
                case YamlClassId.Skybox: return typeof(Skybox);
                case YamlClassId.QualitySettings: return typeof(QualitySettings);
                case YamlClassId.Shader: return typeof(Shader);
                case YamlClassId.TextAsset: return typeof(TextAsset);
                case YamlClassId.Rigidbody2D: return typeof(Rigidbody2D);
                //case YamlClassId.Physics2DManager: return typeof(Physics2DManager);
                case YamlClassId.Collider2D: return typeof(Collider2D);
                case YamlClassId.Rigidbody: return typeof(Rigidbody);
                //case YamlClassId.PhysicsManager: return typeof(PhysicsManager);
                case YamlClassId.Collider: return typeof(Collider);
                case YamlClassId.Joint: return typeof(Joint);
                case YamlClassId.CircleCollider2D: return typeof(CircleCollider2D);
                case YamlClassId.HingeJoint: return typeof(HingeJoint);
                case YamlClassId.PolygonCollider2D: return typeof(PolygonCollider2D);
                case YamlClassId.BoxCollider2D: return typeof(BoxCollider2D);
                case YamlClassId.PhysicsMaterial2D: return typeof(PhysicsMaterial2D);
                case YamlClassId.MeshCollider: return typeof(MeshCollider);
                case YamlClassId.BoxCollider: return typeof(BoxCollider);
                //case YamlClassId.SpriteCollider2D: return typeof(SpriteCollider2D);
                case YamlClassId.EdgeCollider2D: return typeof(EdgeCollider2D);
                case YamlClassId.ComputeShader: return typeof(ComputeShader);
                case YamlClassId.AnimationClip: return typeof(AnimationClip);
                case YamlClassId.ConstantForce: return typeof(ConstantForce);
                //case YamlClassId.WorldParticleCollider: return typeof(WorldParticleCollider);
                //case YamlClassId.TagManager: return typeof(TagManager);
                case YamlClassId.AudioListener: return typeof(AudioListener);
                case YamlClassId.AudioSource: return typeof(AudioSource);
                case YamlClassId.AudioClip: return typeof(AudioClip);
                case YamlClassId.RenderTexture: return typeof(RenderTexture);
                //case YamlClassId.MeshParticleEmitter: return typeof(MeshParticleEmitter);
                //case YamlClassId.ParticleEmitter: return typeof(ParticleEmitter);
                case YamlClassId.Cubemap: return typeof(Cubemap);
                case YamlClassId.Avatar: return typeof(Avatar);
                //case YamlClassId.AnimatorController: return typeof(AnimatorController);
                //case YamlClassId.GUILayer: return typeof(GUILayer);
                case YamlClassId.RuntimeAnimatorController: return typeof(RuntimeAnimatorController);
                //case YamlClassId.ScriptMapper: return typeof(ScriptMapper);
                case YamlClassId.Animator: return typeof(Animator);
                case YamlClassId.TrailRenderer: return typeof(TrailRenderer);
                //case YamlClassId.DelayedCallManager: return typeof(DelayedCallManager);
                case YamlClassId.TextMesh: return typeof(TextMesh);
                case YamlClassId.RenderSettings: return typeof(RenderSettings);
                case YamlClassId.Light: return typeof(Light);
                //case YamlClassId.CGProgram: return typeof(CGProgram);
                //case YamlClassId.BaseAnimationTrack: return typeof(BaseAnimationTrack);
                case YamlClassId.Animation: return typeof(Animation);
                case YamlClassId.MonoBehaviour: return typeof(MonoBehaviour);
                case YamlClassId.MonoScript: return typeof(MonoScript);
                //case YamlClassId.MonoManager: return typeof(MonoManager);
                case YamlClassId.Texture3D: return typeof(Texture3D);
                //case YamlClassId.NewAnimationTrack: return typeof(NewAnimationTrack);
                case YamlClassId.Projector: return typeof(Projector);
                case YamlClassId.LineRenderer: return typeof(LineRenderer);
                case YamlClassId.Flare: return typeof(Flare);
                //case YamlClassId.Halo: return typeof(Halo);
                case YamlClassId.LensFlare: return typeof(LensFlare);
                case YamlClassId.FlareLayer: return typeof(FlareLayer);
                //case YamlClassId.HaloLayer: return typeof(HaloLayer);
                //case YamlClassId.NavMeshAreas: return typeof(NavMeshAreas);
                //case YamlClassId.HaloManager: return typeof(HaloManager);
                case YamlClassId.Font: return typeof(Font);
                case YamlClassId.PlayerSettings: return typeof(PlayerSettings);
                //case YamlClassId.NamedObject: return typeof(NamedObject);
                //case YamlClassId.GUITexture: return typeof(GUITexture);
                //case YamlClassId.GUIText: return typeof(GUIText);
                //case YamlClassId.GUIElement: return typeof(GUIElement);
                case YamlClassId.PhysicMaterial: return typeof(PhysicMaterial);
                case YamlClassId.SphereCollider: return typeof(SphereCollider);
                case YamlClassId.CapsuleCollider: return typeof(CapsuleCollider);
                case YamlClassId.SkinnedMeshRenderer: return typeof(SkinnedMeshRenderer);
                case YamlClassId.FixedJoint: return typeof(FixedJoint);
                //case YamlClassId.RaycastCollider: return typeof(RaycastCollider);
                //case YamlClassId.BuildSettings: return typeof(BuildSettings);
                case YamlClassId.AssetBundle: return typeof(AssetBundle);
                case YamlClassId.CharacterController: return typeof(CharacterController);
                case YamlClassId.CharacterJoint: return typeof(CharacterJoint);
                case YamlClassId.SpringJoint: return typeof(SpringJoint);
                case YamlClassId.WheelCollider: return typeof(WheelCollider);
                //case YamlClassId.ResourceManager: return typeof(ResourceManager);
                //case YamlClassId.NetworkView: return typeof(NetworkView);
                //case YamlClassId.NetworkManager: return typeof(NetworkManager);
                //case YamlClassId.PreloadData: return typeof(PreloadData);
                //case YamlClassId.MovieTexture: return typeof(MovieTexture);
                case YamlClassId.ConfigurableJoint: return typeof(ConfigurableJoint);
                case YamlClassId.TerrainCollider: return typeof(TerrainCollider);
                //case YamlClassId.MasterServerInterface: return typeof(MasterServerInterface);
                case YamlClassId.TerrainData: return typeof(TerrainData);
                case YamlClassId.LightmapSettings: return typeof(LightmapSettings);
                case YamlClassId.WebCamTexture: return typeof(WebCamTexture);
                case YamlClassId.EditorSettings: return typeof(EditorSettings);
                //case YamlClassId.InteractiveCloth: return typeof(InteractiveCloth);
                //case YamlClassId.ClothRenderer: return typeof(ClothRenderer);
                case YamlClassId.EditorUserSettings: return typeof(EditorUserSettings);
                //case YamlClassId.SkinnedCloth: return typeof(SkinnedCloth);
                case YamlClassId.AudioReverbFilter: return typeof(AudioReverbFilter);
                case YamlClassId.AudioHighPassFilter: return typeof(AudioHighPassFilter);
                case YamlClassId.AudioChorusFilter: return typeof(AudioChorusFilter);
                case YamlClassId.AudioReverbZone: return typeof(AudioReverbZone);
                case YamlClassId.AudioEchoFilter: return typeof(AudioEchoFilter);
                case YamlClassId.AudioLowPassFilter: return typeof(AudioLowPassFilter);
                case YamlClassId.AudioDistortionFilter: return typeof(AudioDistortionFilter);
                case YamlClassId.SparseTexture: return typeof(SparseTexture);
                case YamlClassId.AudioBehaviour: return typeof(AudioBehaviour);
                //case YamlClassId.AudioFilter: return typeof(AudioFilter);
                case YamlClassId.WindZone: return typeof(WindZone);
                case YamlClassId.Cloth: return typeof(Cloth);
                //case YamlClassId.SubstanceArchive: return typeof(SubstanceArchive);
                //case YamlClassId.ProceduralMaterial: return typeof(ProceduralMaterial);
                //case YamlClassId.ProceduralTexture: return typeof(ProceduralTexture);
                case YamlClassId.OffMeshLink: return typeof(OffMeshLink);
                case YamlClassId.OcclusionArea: return typeof(OcclusionArea);
                case YamlClassId.Tree: return typeof(Tree);
                //case YamlClassId.NavMeshObsolete: return typeof(NavMeshObsolete);
                case YamlClassId.NavMeshAgent: return typeof(NavMeshAgent);
                //case YamlClassId.NavMeshSettings: return typeof(NavMeshSettings);
                //case YamlClassId.LightProbesLegacy: return typeof(LightProbesLegacy);
                case YamlClassId.ParticleSystem: return typeof(ParticleSystem);
                case YamlClassId.ParticleSystemRenderer: return typeof(ParticleSystemRenderer);
                case YamlClassId.ShaderVariantCollection: return typeof(ShaderVariantCollection);
                case YamlClassId.LODGroup: return typeof(LODGroup);
                case YamlClassId.BlendTree: return typeof(BlendTree);
                case YamlClassId.Motion: return typeof(Motion);
                case YamlClassId.NavMeshObstacle: return typeof(NavMeshObstacle);
                //case YamlClassId.TerrainInstance: return typeof(TerrainInstance);
                case YamlClassId.SpriteRenderer: return typeof(SpriteRenderer);
                case YamlClassId.Sprite: return typeof(Sprite);
                //case YamlClassId.CachedSpriteAtlas: return typeof(CachedSpriteAtlas);
                case YamlClassId.ReflectionProbe: return typeof(ReflectionProbe);
                //case YamlClassId.ReflectionProbes: return typeof(ReflectionProbes);
                case YamlClassId.Terrain: return typeof(Terrain);
                case YamlClassId.LightProbeGroup: return typeof(LightProbeGroup);
                case YamlClassId.AnimatorOverrideController: return typeof(AnimatorOverrideController);
                case YamlClassId.CanvasRenderer: return typeof(CanvasRenderer);
                case YamlClassId.Canvas: return typeof(Canvas);
                case YamlClassId.RectTransform: return typeof(RectTransform);
                case YamlClassId.CanvasGroup: return typeof(CanvasGroup);
                case YamlClassId.BillboardAsset: return typeof(BillboardAsset);
                case YamlClassId.BillboardRenderer: return typeof(BillboardRenderer);
                //case YamlClassId.SpeedTreeWindAsset: return typeof(SpeedTreeWindAsset);
                case YamlClassId.AnchoredJoint2D: return typeof(AnchoredJoint2D);
                case YamlClassId.Joint2D: return typeof(Joint2D);
                case YamlClassId.SpringJoint2D: return typeof(SpringJoint2D);
                case YamlClassId.DistanceJoint2D: return typeof(DistanceJoint2D);
                case YamlClassId.HingeJoint2D: return typeof(HingeJoint2D);
                case YamlClassId.SliderJoint2D: return typeof(SliderJoint2D);
                case YamlClassId.WheelJoint2D: return typeof(WheelJoint2D);
                case YamlClassId.NavMeshData: return typeof(NavMeshData);
                //case YamlClassId.AudioMixer: return typeof(AudioMixer);
                //case YamlClassId.AudioMixerController: return typeof(AudioMixerController);
                //case YamlClassId.AudioMixerGroupController: return typeof(AudioMixerGroupController);
                //case YamlClassId.AudioMixerEffectController: return typeof(AudioMixerEffectController);
                //case YamlClassId.AudioMixerSnapshotController: return typeof(AudioMixerSnapshotController);
                case YamlClassId.PhysicsUpdateBehaviour2D: return typeof(PhysicsUpdateBehaviour2D);
                case YamlClassId.ConstantForce2D: return typeof(ConstantForce2D);
                case YamlClassId.Effector2D: return typeof(Effector2D);
                case YamlClassId.AreaEffector2D: return typeof(AreaEffector2D);
                case YamlClassId.PointEffector2D: return typeof(PointEffector2D);
                case YamlClassId.PlatformEffector2D: return typeof(PlatformEffector2D);
                case YamlClassId.SurfaceEffector2D: return typeof(SurfaceEffector2D);
                case YamlClassId.LightProbes: return typeof(LightProbes);
                //case YamlClassId.SampleClip: return typeof(SampleClip);
                case YamlClassId.AudioMixerSnapshot: return typeof(AudioMixerSnapshot);
                case YamlClassId.AudioMixerGroup: return typeof(AudioMixerGroup);
                case YamlClassId.AssetBundleManifest: return typeof(AssetBundleManifest);
                //case YamlClassId.Prefab: return typeof(Prefab);
                //case YamlClassId.EditorExtensionImpl: return typeof(EditorExtensionImpl);
                case YamlClassId.AssetImporter: return typeof(AssetImporter);
                case YamlClassId.AssetDatabase: return typeof(AssetDatabase);
                //case YamlClassId.Mesh3DSImporter: return typeof(Mesh3DSImporter);
                case YamlClassId.TextureImporter: return typeof(TextureImporter);
                case YamlClassId.ShaderImporter: return typeof(ShaderImporter);
                //case YamlClassId.ComputeShaderImporter: return typeof(ComputeShaderImporter);
                case YamlClassId.AvatarMask: return typeof(AvatarMask);
                case YamlClassId.AudioImporter: return typeof(AudioImporter);
                //case YamlClassId.HierarchyState: return typeof(HierarchyState);
                //case YamlClassId.GUIDSerializer: return typeof(GUIDSerializer);
                //case YamlClassId.AssetMetaData: return typeof(AssetMetaData);
                case YamlClassId.DefaultAsset: return typeof(DefaultAsset);
                //case YamlClassId.DefaultImporter: return typeof(DefaultImporter);
                //case YamlClassId.TextScriptImporter: return typeof(TextScriptImporter);
                case YamlClassId.SceneAsset: return typeof(SceneAsset);
                //case YamlClassId.NativeFormatImporter: return typeof(NativeFormatImporter);
                case YamlClassId.MonoImporter: return typeof(MonoImporter);
                //case YamlClassId.AssetServerCache: return typeof(AssetServerCache);
                //case YamlClassId.LibraryAssetImporter: return typeof(LibraryAssetImporter);
                case YamlClassId.ModelImporter: return typeof(ModelImporter);
                //case YamlClassId.FBXImporter: return typeof(FBXImporter);
                case YamlClassId.TrueTypeFontImporter: return typeof(TrueTypeFontImporter);
                //case YamlClassId.MovieImporter: return typeof(MovieImporter);
                case YamlClassId.EditorBuildSettings: return typeof(EditorBuildSettings);
                //case YamlClassId.DDSImporter: return typeof(DDSImporter);
                //case YamlClassId.InspectorExpandedState: return typeof(InspectorExpandedState);
                //case YamlClassId.AnnotationManager: return typeof(AnnotationManager);
                case YamlClassId.PluginImporter: return typeof(PluginImporter);
                case YamlClassId.EditorUserBuildSettings: return typeof(EditorUserBuildSettings);
                //case YamlClassId.PVRImporter: return typeof(PVRImporter);
                //case YamlClassId.ASTCImporter: return typeof(ASTCImporter);
                //case YamlClassId.KTXImporter: return typeof(KTXImporter);
                case YamlClassId.AnimatorStateTransition: return typeof(AnimatorStateTransition);
                case YamlClassId.AnimatorState: return typeof(AnimatorState);
                case YamlClassId.HumanTemplate: return typeof(HumanTemplate);
                case YamlClassId.AnimatorStateMachine: return typeof(AnimatorStateMachine);
                //case YamlClassId.PreviewAssetType: return typeof(PreviewAssetType);
                case YamlClassId.AnimatorTransition: return typeof(AnimatorTransition);
                case YamlClassId.SpeedTreeImporter: return typeof(SpeedTreeImporter);
                case YamlClassId.AnimatorTransitionBase: return typeof(AnimatorTransitionBase);
                //case YamlClassId.SubstanceImporter: return typeof(SubstanceImporter);
                case YamlClassId.LightmapParameters: return typeof(LightmapParameters);
                //case YamlClassId.LightmapSnapshot: return typeof(LightmapSnapshot);

                //case YamlClassId.SketchUpImporter: return typeof(SketchUpImporter);
                //case YamlClassId.BuildReport: return typeof(BuildReport);
                //case YamlClassId.PackedAssets: return typeof(PackedAssets);
                case YamlClassId.VideoClipImporter: return typeof(VideoClipImporter);
                //case YamlClassId.int: return typeof(int);
                //case YamlClassId.bool: return typeof(bool);
                //case YamlClassId.float: return typeof(float);
                //case YamlClassId.MonoObject: return typeof(MonoObject);
                case YamlClassId.Collision: return typeof(Collision);
                //case YamlClassId.Vector3f: return typeof(Vector3f);
                //case YamlClassId.RootMotionData: return typeof(RootMotionData);
                case YamlClassId.Collision2D: return typeof(Collision2D);
                //case YamlClassId.AudioMixerLiveUpdateFloat: return typeof(AudioMixerLiveUpdateFloat);
                //case YamlClassId.AudioMixerLiveUpdateBool: return typeof(AudioMixerLiveUpdateBool);
                //case YamlClassId.Polygon2D: return typeof(Polygon2D);
                //case YamlClassId.void: return typeof(void);
                //case YamlClassId.TilemapCollider2D: return typeof(TilemapCollider2D);
                //case YamlClassId.AssetImporterLog: return typeof(AssetImporterLog);
                //case YamlClassId.VFXRenderer: return typeof(VFXRenderer);
                //case YamlClassId.SerializableManagedRefTestClass: return typeof(SerializableManagedRefTestClass);
                //case YamlClassId.Grid: return typeof(Grid);
                //case YamlClassId.Preset: return typeof(Preset);
                //case YamlClassId.EmptyObject: return typeof(EmptyObject);
                //case YamlClassId.IConstraint: return typeof(IConstraint);
                //case YamlClassId.TestObjectWithSpecialLayoutOne: return typeof(TestObjectWithSpecialLayoutOne);
                //case YamlClassId.AssemblyDefinitionReferenceImporter: return typeof(AssemblyDefinitionReferenceImporter);
                //case YamlClassId.SiblingDerived: return typeof(SiblingDerived);
                //case YamlClassId.TestObjectWithSerializedMapStringNonAlignedStruct: return typeof(TestObjectWithSerializedMapStringNonAlignedStruct);
                //case YamlClassId.SubDerived: return typeof(SubDerived);
                //case YamlClassId.AssetImportInProgressProxy: return typeof(AssetImportInProgressProxy);
                //case YamlClassId.PluginBuildInfo: return typeof(PluginBuildInfo);
                //case YamlClassId.EditorProjectAccess: return typeof(EditorProjectAccess);
                //case YamlClassId.PrefabImporter: return typeof(PrefabImporter);
                //case YamlClassId.TestObjectWithSerializedArray: return typeof(TestObjectWithSerializedArray);
                //case YamlClassId.TestObjectWithSerializedAnimationCurve: return typeof(TestObjectWithSerializedAnimationCurve);
                //case YamlClassId.TilemapRenderer: return typeof(TilemapRenderer);
                //case YamlClassId.SpriteAtlasDatabase: return typeof(SpriteAtlasDatabase);
                //case YamlClassId.AudioBuildInfo: return typeof(AudioBuildInfo);
                //case YamlClassId.CachedSpriteAtlasRuntimeData: return typeof(CachedSpriteAtlasRuntimeData);
                //case YamlClassId.RendererFake: return typeof(RendererFake);
                //case YamlClassId.AssemblyDefinitionReferenceAsset: return typeof(AssemblyDefinitionReferenceAsset);
                //case YamlClassId.BuiltAssetBundleInfoSet: return typeof(BuiltAssetBundleInfoSet);
                case YamlClassId.SpriteAtlas: return typeof(SpriteAtlas);
                //case YamlClassId.RayTracingShaderImporter: return typeof(RayTracingShaderImporter);
                //case YamlClassId.RayTracingShader: return typeof(RayTracingShader);
                //case YamlClassId.PlatformModuleSetup: return typeof(PlatformModuleSetup);
                //case YamlClassId.AimConstraint: return typeof(AimConstraint);
                //case YamlClassId.VFXManager: return typeof(VFXManager);
                //case YamlClassId.VisualEffectSubgraph: return typeof(VisualEffectSubgraph);
                //case YamlClassId.VisualEffectSubgraphOperator: return typeof(VisualEffectSubgraphOperator);
                //case YamlClassId.VisualEffectSubgraphBlock: return typeof(VisualEffectSubgraphBlock);
                //case YamlClassId.Prefab: return typeof(Prefab);
                //case YamlClassId.LocalizationImporter: return typeof(LocalizationImporter);
                //case YamlClassId.Derived: return typeof(Derived);
                //case YamlClassId.PropertyModificationsTargetTestObject: return typeof(PropertyModificationsTargetTestObject);
                //case YamlClassId.ReferencesArtifactGenerator: return typeof(ReferencesArtifactGenerator);
                //case YamlClassId.AssemblyDefinitionAsset: return typeof(AssemblyDefinitionAsset);
                //case YamlClassId.SceneVisibilityState: return typeof(SceneVisibilityState);
                //case YamlClassId.LookAtConstraint: return typeof(LookAtConstraint);
                //case YamlClassId.MultiArtifactTestImporter: return typeof(MultiArtifactTestImporter);
                //case YamlClassId.GameObjectRecorder: return typeof(GameObjectRecorder);
                //case YamlClassId.LightingDataAssetParent: return typeof(LightingDataAssetParent);
                //case YamlClassId.PresetManager: return typeof(PresetManager);
                //case YamlClassId.TestObjectWithSpecialLayoutTwo: return typeof(TestObjectWithSpecialLayoutTwo);
                //case YamlClassId.StreamingManager: return typeof(StreamingManager);
                //case YamlClassId.LowerResBlitTexture: return typeof(LowerResBlitTexture);
                //case YamlClassId.StreamingController: return typeof(StreamingController);
                //case YamlClassId.TestObjectVectorPairStringBool: return typeof(TestObjectVectorPairStringBool);
                //case YamlClassId.GridLayout: return typeof(GridLayout);
                //case YamlClassId.AssemblyDefinitionImporter: return typeof(AssemblyDefinitionImporter);
                //case YamlClassId.ParentConstraint: return typeof(ParentConstraint);
                //case YamlClassId.FakeComponent: return typeof(FakeComponent);
                //case YamlClassId.PositionConstraint: return typeof(PositionConstraint);
                //case YamlClassId.RotationConstraint: return typeof(RotationConstraint);
                //case YamlClassId.ScaleConstraint: return typeof(ScaleConstraint);
                //case YamlClassId.Tilemap: return typeof(Tilemap);
                //case YamlClassId.PackageManifest: return typeof(PackageManifest);
                //case YamlClassId.PackageManifestImporter: return typeof(PackageManifestImporter);
                //case YamlClassId.TerrainLayer: return typeof(TerrainLayer);
                //case YamlClassId.SpriteShapeRenderer: return typeof(SpriteShapeRenderer);
                //case YamlClassId.NativeObjectType: return typeof(NativeObjectType);
                //case YamlClassId.TestObjectWithSerializedMapStringBool: return typeof(TestObjectWithSerializedMapStringBool);
                //case YamlClassId.SerializableManagedHost: return typeof(SerializableManagedHost);
                //case YamlClassId.VisualEffectAsset: return typeof(VisualEffectAsset);
                //case YamlClassId.VisualEffectImporter: return typeof(VisualEffectImporter);
                //case YamlClassId.VisualEffectResource: return typeof(VisualEffectResource);
                //case YamlClassId.VisualEffectObject: return typeof(VisualEffectObject);
                //case YamlClassId.VisualEffect: return typeof(VisualEffect);
                //case YamlClassId.LocalizationAsset: return typeof(LocalizationAsset);
                //case YamlClassId.ScriptedImporter: return typeof(ScriptedImporter);

                default: return null;
            }
        }
#endif
            }
        }
