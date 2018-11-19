using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UTJ.MeshSync
{
    public static partial class Misc
    {
        public const int InvalidID = -1;

        public static string S(IntPtr cstring)
        {
            return cstring == IntPtr.Zero ? "" : Marshal.PtrToStringAnsi(cstring);
        }

        public static string SanitizeFileName(string name)
        {
            var reg = new Regex("[:<>|\\*\\?]");
            return reg.Replace(name, "_");
        }

        public static void Resize<T>(List<T> list, int n) where T : new()
        {
            int cur = list.Count;
            if (n < cur)
                list.RemoveRange(n, cur - n);
            else if (n > cur)
            {
                if (n > list.Capacity)
                    list.Capacity = n;
                int a = n - cur;
                for (int i = 0; i < a; ++i)
                    list.Add(new T());
            }
        }

        public static T GetOrAddComponent<T>(GameObject go) where T : Component
        {
            var ret = go.GetComponent<T>();
            if (ret == null)
                ret = go.AddComponent<T>();
            return ret;
        }

        public static UnityEngine.TextureFormat ToUnityTextureFormat(TextureFormat v)
        {
            switch (v)
            {
                case TextureFormat.Ru8: return UnityEngine.TextureFormat.R8;
                case TextureFormat.RGu8: return UnityEngine.TextureFormat.RG16;
                case TextureFormat.RGBu8: return UnityEngine.TextureFormat.RGB24;
                case TextureFormat.RGBAu8: return UnityEngine.TextureFormat.RGBA32;
                case TextureFormat.Rf16: return UnityEngine.TextureFormat.RHalf;
                case TextureFormat.RGf16: return UnityEngine.TextureFormat.RGHalf;
                case TextureFormat.RGBAf16: return UnityEngine.TextureFormat.RGBAHalf;
                case TextureFormat.Rf32: return UnityEngine.TextureFormat.RFloat;
                case TextureFormat.RGf32: return UnityEngine.TextureFormat.RGFloat;
                case TextureFormat.RGBAf32: return UnityEngine.TextureFormat.RGBAFloat;
                default: return UnityEngine.TextureFormat.Alpha8;
            }
        }

        public static TextureFormat ToMSTextureFormat(UnityEngine.TextureFormat v)
        {
            switch (v)
            {
                case UnityEngine.TextureFormat.R8: return TextureFormat.Ru8;
                case UnityEngine.TextureFormat.RG16: return TextureFormat.RGu8;
                case UnityEngine.TextureFormat.RGB24: return TextureFormat.RGBu8;
                case UnityEngine.TextureFormat.RGBA32: return TextureFormat.RGBAu8;
                case UnityEngine.TextureFormat.RHalf: return TextureFormat.Rf16;
                case UnityEngine.TextureFormat.RGHalf: return TextureFormat.RGf16;
                case UnityEngine.TextureFormat.RGBAHalf: return TextureFormat.RGBAf16;
                case UnityEngine.TextureFormat.RFloat: return TextureFormat.Rf32;
                case UnityEngine.TextureFormat.RGFloat: return TextureFormat.RGf32;
                case UnityEngine.TextureFormat.RGBAFloat: return TextureFormat.RGBAf32;
                default: return TextureFormat.Ru8;
            }
        }
    }


    #region Server
    public struct ServerSettings
    {
        public int maxQueue;
        public int maxThreads;
        public ushort port;
        public uint meshSplitUnit;

        public static ServerSettings defaultValue
        {
            get
            {
                return new ServerSettings
                {
                    maxQueue = 512,
                    maxThreads = 8,
                    port = 8080,
#if UNITY_2017_3_OR_NEWER
                    meshSplitUnit = 0xffffffff,
#else
                    meshSplitUnit = 65000,
#endif
                };
            }
        }
    }
    public struct Server
    {
        #region internal
        internal IntPtr _this;
        [DllImport("MeshSyncServer")] static extern IntPtr msServerGetVersion();
        [DllImport("MeshSyncServer")] static extern Server msServerStart(ref ServerSettings settings);
        [DllImport("MeshSyncServer")] static extern void msServerStop(IntPtr _this);

        [DllImport("MeshSyncServer")] static extern int msServerGetNumMessages(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern int msServerProcessMessages(IntPtr _this, MessageHandler handler);

        [DllImport("MeshSyncServer")] static extern void msServerBeginServe(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern void msServerEndServe(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern void msServerServeTransform(IntPtr _this, TransformData data);
        [DllImport("MeshSyncServer")] static extern void msServerServeCamera(IntPtr _this, CameraData data);
        [DllImport("MeshSyncServer")] static extern void msServerServeLight(IntPtr _this, LightData data);
        [DllImport("MeshSyncServer")] static extern void msServerServeMesh(IntPtr _this, MeshData data);
        [DllImport("MeshSyncServer")] static extern void msServerServeTexture(IntPtr _this, TextureData data);
        [DllImport("MeshSyncServer")] static extern void msServerServeMaterial(IntPtr _this, MaterialData data);
        [DllImport("MeshSyncServer")] static extern void msServerSetFileRootPath(IntPtr _this, string path);
        [DllImport("MeshSyncServer")] static extern void msServerSetScreenshotFilePath(IntPtr _this, string path);
        [DllImport("MeshSyncServer")] static extern void msServerNotifyPoll(IntPtr _this, PollMessage.PollType t);
        #endregion

        public delegate void MessageHandler(MessageType type, IntPtr data);

        public static implicit operator bool(Server v) { return v._this != IntPtr.Zero; }

        public static string version { get { return Misc.S(msServerGetVersion()); } }

        public static Server Start(ref ServerSettings settings) { return msServerStart(ref settings); }
        public void Stop() { msServerStop(_this); }

        public int numMessages { get { return msServerGetNumMessages(_this); } }
        public void ProcessMessages(MessageHandler handler) { msServerProcessMessages(_this, handler); }

        public string fileRootPath { set { msServerSetFileRootPath(_this, value); } }
        public string screenshotPath { set { msServerSetScreenshotFilePath(_this, value); } }

        public void BeginServe() { msServerBeginServe(_this); }
        public void EndServe() { msServerEndServe(_this); }
        public void ServeTransform(TransformData data) { msServerServeTransform(_this, data); }
        public void ServeCamera(CameraData data) { msServerServeCamera(_this, data); }
        public void ServeLight(LightData data) { msServerServeLight(_this, data); }
        public void ServeMesh(MeshData data) { msServerServeMesh(_this, data); }
        public void ServeTexture(TextureData data) { msServerServeTexture(_this, data); }
        public void ServeMaterial(MaterialData data) { msServerServeMaterial(_this, data); }
        public void NotifyPoll(PollMessage.PollType t) { msServerNotifyPoll(_this, t); }
    }
    #endregion


    #region Messages
    public enum MessageType
    {
        Unknown,
        Get,
        Set,
        Delete,
        Fence,
        Text,
        Screenshot,
        Query,
        Response,
    }

    public struct GetFlags
    {
        public BitFlags flags;
        public bool getTransform { get { return flags[0]; } }
        public bool getPoints { get { return flags[1]; } }
        public bool getNormals { get { return flags[2]; } }
        public bool getTangents { get { return flags[3]; } }
        public bool getUV0 { get { return flags[4]; } }
        public bool getUV1 { get { return flags[5]; } }
        public bool getColors { get { return flags[6]; } }
        public bool getIndices { get { return flags[7]; } }
        public bool getMaterialIDs { get { return flags[8]; } }
        public bool getBones { get { return flags[9]; } }
        public bool getBlendShapes { get { return flags[10]; } }
    }

    public struct GetMessage
    {
        #region internal
        internal IntPtr _this;
        [DllImport("MeshSyncServer")] static extern GetFlags msGetGetFlags(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern int msGetGetBakeSkin(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern int msGetGetBakeCloth(IntPtr _this);
        #endregion

        public static explicit operator GetMessage(IntPtr v)
        {
            GetMessage ret;
            ret._this = v;
            return ret;
        }

        public GetFlags flags { get { return msGetGetFlags(_this); } }
        public bool bakeSkin { get { return msGetGetBakeSkin(_this) != 0; } }
        public bool bakeCloth { get { return msGetGetBakeCloth(_this) != 0; } }
    }

    public struct SetMessage
    {
        #region internal
        internal IntPtr _this;
        [DllImport("MeshSyncServer")] static extern SceneData msSetGetSceneData(IntPtr _this);
        #endregion

        public static explicit operator SetMessage(IntPtr v)
        {
            SetMessage ret;
            ret._this = v;
            return ret;
        }

        public SceneData scene
        {
            get { return msSetGetSceneData(_this); }
        }
    }

    public struct DeleteMessage
    {
        #region internal
        internal IntPtr _this;
        [DllImport("MeshSyncServer")] static extern int msDeleteGetNumEntities(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern Identifier msDeleteGetEntity(IntPtr _this, int i);
        [DllImport("MeshSyncServer")] static extern int msDeleteGetNumMaterials(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern Identifier msDeleteGetMaterial(IntPtr _this, int i);
        #endregion

        public static explicit operator DeleteMessage(IntPtr v)
        {
            DeleteMessage ret;
            ret._this = v;
            return ret;
        }

        public int numEntities { get { return msDeleteGetNumEntities(_this); } }
        public Identifier GetEntity(int i) { return msDeleteGetEntity(_this, i); }

        public int numMaterials { get { return msDeleteGetNumMaterials(_this); } }
        public Identifier GetMaterial(int i) { return msDeleteGetMaterial(_this, i); }
    }

    public struct FenceMessage
    {
        #region internal
        internal IntPtr _this;
        [DllImport("MeshSyncServer")] static extern FenceType msFenceGetType(IntPtr _this);
        #endregion

        public enum FenceType
        {
            Unknown,
            SceneBegin,
            SceneEnd,
        }

        public static explicit operator FenceMessage(IntPtr v)
        {
            FenceMessage ret;
            ret._this = v;
            return ret;
        }

        public FenceType type { get { return msFenceGetType(_this); } }
    }

    public struct TextMessage
    {
        #region internal
        internal IntPtr _this;
        [DllImport("MeshSyncServer")] static extern IntPtr msTextGetText(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern TextType msTextGetType(IntPtr _this);
        #endregion

        public enum TextType
        {
            Normal,
            Warning,
            Error,
        }

        public static explicit operator TextMessage(IntPtr v)
        {
            TextMessage ret;
            ret._this = v;
            return ret;
        }

        public string text { get { return Misc.S(msTextGetText(_this)); } }
        public TextType textType { get { return msTextGetType(_this); } }

        public void Print()
        {
            switch (textType)
            {
                case TextType.Error:
                    Debug.LogError(text);
                    break;
                case TextType.Warning:
                    Debug.LogWarning(text);
                    break;
                default:
                    Debug.Log(text);
                    break;
            }

        }
    }

    public struct QueryMessage
    {
        #region internal
        internal IntPtr _this;
        [DllImport("MeshSyncServer")] static extern QueryType msQueryGetType(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern void msQueryFinishRespond(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern void msQueryAddResponseText(IntPtr _this, string text);
        #endregion

        public enum QueryType
        {
            Unknown,
            ClientName,
            RootNodes,
            AllNodes,
        }

        public static explicit operator QueryMessage(IntPtr v)
        {
            QueryMessage ret;
            ret._this = v;
            return ret;
        }

        public QueryType queryType { get { return msQueryGetType(_this); } }

        public void FinishRespond()
        {
            msQueryFinishRespond(_this);
        }
        public void AddResponseText(string text)
        {
            msQueryAddResponseText(_this, text);
        }
    }

    public struct PollMessage
    {
        public enum PollType
        {
            Unknown,
            SceneUpdate,
        }
    }
    #endregion


    #region Material
    public enum TextureType
    {
        Default,
        NormalMap,
    }

    public enum TextureFormat
    {
        Unknown = 0,

        ChannelMask = 0xF,
        TypeMask = 0xF << 4,
        Type_f16 = 0x1 << 4,
        Type_f32 = 0x2 << 4,
        Type_u8 = 0x3 << 4,
        Type_i16 = 0x4 << 4,
        Type_i32 = 0x5 << 4,

        Rf16 = Type_f16 | 1,
        RGf16 = Type_f16 | 2,
        RGBf16 = Type_f16 | 3,
        RGBAf16 = Type_f16 | 4,
        Rf32 = Type_f32 | 1,
        RGf32 = Type_f32 | 2,
        RGBf32 = Type_f32 | 3,
        RGBAf32 = Type_f32 | 4,
        Ru8 = Type_u8 | 1,
        RGu8 = Type_u8 | 2,
        RGBu8 = Type_u8 | 3,
        RGBAu8 = Type_u8 | 4,
        Ri16 = Type_i16 | 1,
        RGi16 = Type_i16 | 2,
        RGBi16 = Type_i16 | 3,
        RGBAi16 = Type_i16 | 4,
        Ri32 = Type_i32 | 1,
        RGi32 = Type_i32 | 2,
        RGBi32 = Type_i32 | 3,
        RGBAi32 = Type_i32 | 4,

        RawFile = 0x10 << 4,
    }

    public struct TextureData
    {
        #region internal
        internal IntPtr _this;
        [DllImport("MeshSyncServer")] static extern TextureData msTextureCreate();
        [DllImport("MeshSyncServer")] static extern int msTextureGetID(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern void msTextureSetID(IntPtr _this, int v);
        [DllImport("MeshSyncServer")] static extern IntPtr msTextureGetName(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern void msTextureSetName(IntPtr _this, string v);
        [DllImport("MeshSyncServer")] static extern TextureType msTextureGetType(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern void msTextureSetType(IntPtr _this, TextureType v);
        [DllImport("MeshSyncServer")] static extern TextureFormat msTextureGetFormat(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern void msTextureSetFormat(IntPtr _this, TextureFormat v);
        [DllImport("MeshSyncServer")] static extern int msTextureGetWidth(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern void msTextureSetWidth(IntPtr _this, int v);
        [DllImport("MeshSyncServer")] static extern int msTextureGetHeight(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern void msTextureSetHeight(IntPtr _this, int v);
        [DllImport("MeshSyncServer")] static extern IntPtr msTextureGetDataPtr(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern int msTextureGetSizeInByte(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern byte msTextureWriteToFile(IntPtr _this, string path);
        [DllImport("MeshSyncServer")] static extern byte msWriteToFile(string path, byte[] data, int size);
        #endregion

        public static TextureData Create() { return msTextureCreate(); }

        public int id
        {
            get { return msTextureGetID(_this); }
            set { msTextureSetID(_this, value); }
        }
        public string name
        {
            get { return Misc.S(msTextureGetName(_this)); }
            set { msTextureSetName(_this, value); }
        }
        public TextureType type
        {
            get { return msTextureGetType(_this); }
            set { msTextureSetType(_this, value); }
        }
        public TextureFormat format
        {
            get { return msTextureGetFormat(_this); }
            set { msTextureSetFormat(_this, value); }
        }
        public int width
        {
            get { return msTextureGetWidth(_this); }
            set { msTextureSetWidth(_this, value); }
        }
        public int height
        {
            get { return msTextureGetHeight(_this); }
            set { msTextureSetHeight(_this, value); }
        }
        public int sizeInByte
        {
            get { return msTextureGetSizeInByte(_this); }
        }
        public IntPtr dataPtr
        {
            get { return msTextureGetDataPtr(_this); }
        }

        public bool WriteToFile(string path)
        {
            return msTextureWriteToFile(_this, path) != 0;
        }
        public static bool WriteToFile(string path, byte[] data)
        {
            return msWriteToFile(path, data, data.Length) != 0;
        }
    }

    public struct MaterialPropertyData
    {
        #region internal
        internal IntPtr _this;
        [DllImport("MeshSyncServer")] static extern IntPtr msMaterialPropGetName(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern Type msMaterialPropGetType(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern int msMaterialPropGetInt(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern float msMaterialPropGetFloat(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern Vector4 msMaterialPropGetVector(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern Matrix4x4 msMaterialPropGetMatrix(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern int msMaterialPropGetTexture(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern int msMaterialPropGetArraySize(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern void msMaterialPropCopyData(IntPtr _this, float[] dst);
        [DllImport("MeshSyncServer")] static extern void msMaterialPropCopyData(IntPtr _this, Vector4[] dst);
        [DllImport("MeshSyncServer")] static extern void msMaterialPropCopyData(IntPtr _this, Matrix4x4[] dst);
        #endregion

        public enum Type
        {
            Unknown,
            Int,
            Float,
            Vector,
            Matrix,
            FloatArray,
            VectorArray,
            MatrixArray,
            Texture,
        }

        public static implicit operator bool(MaterialPropertyData v)
        {
            return v._this != IntPtr.Zero;
        }

        public string name { get { return Misc.S(msMaterialPropGetName(_this)); } }
        public Type type { get { return msMaterialPropGetType(_this); } }

        public int intValue { get { return msMaterialPropGetInt(_this); } }
        public float floatValue { get { return msMaterialPropGetFloat(_this); } }
        public Vector4 vectorValue { get { return msMaterialPropGetVector(_this); } }
        public Matrix4x4 matrixValue { get { return msMaterialPropGetMatrix(_this); } }
        public float[] floatArray
        {
            get
            {
                var ret = new float[msMaterialPropGetArraySize(_this)];
                msMaterialPropCopyData(_this, ret);
                return ret;
            }
        }
        public Vector4[] vectorArray
        {
            get
            {
                var ret = new Vector4[msMaterialPropGetArraySize(_this)];
                msMaterialPropCopyData(_this, ret);
                return ret;
            }
        }
        public Matrix4x4[] matrixArray
        {
            get
            {
                var ret = new Matrix4x4[msMaterialPropGetArraySize(_this)];
                msMaterialPropCopyData(_this, ret);
                return ret;
            }
        }
        public int textureValue { get { return msMaterialPropGetTexture(_this); } }
    }

    public struct MaterialKeywordData
    {
        #region internal
        internal IntPtr _this;
        [DllImport("MeshSyncServer")] static extern IntPtr msMaterialKeywordGetName(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern byte msMaterialKeywordGetValue(IntPtr _this);
        #endregion

        public static implicit operator bool(MaterialKeywordData v)
        {
            return v._this != IntPtr.Zero;
        }

        public string name { get { return Misc.S(msMaterialKeywordGetName(_this)); } }
        public bool value { get { return msMaterialKeywordGetValue(_this) != 0; } }
    }

    public struct MaterialData
    {
        #region internal
        internal IntPtr _this;
        [DllImport("MeshSyncServer")] static extern MaterialData msMaterialCreate();
        [DllImport("MeshSyncServer")] static extern int msMaterialGetID(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern void msMaterialSetID(IntPtr _this, int v);
        [DllImport("MeshSyncServer")] static extern IntPtr msMaterialGetName(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern void msMaterialSetName(IntPtr _this, string v);
        [DllImport("MeshSyncServer")] static extern int msMaterialGetIndex(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern void msMaterialSetIndex(IntPtr _this, int v);
        [DllImport("MeshSyncServer")] static extern IntPtr msMaterialGetShader(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern void msMaterialSetShader(IntPtr _this, string v);
        [DllImport("MeshSyncServer")] static extern int msMaterialGetNumParams(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern MaterialPropertyData msMaterialGetParam(IntPtr _this, int i);
        [DllImport("MeshSyncServer")] static extern MaterialPropertyData msMaterialFindParam(IntPtr _this, string name);

        [DllImport("MeshSyncServer")] static extern void msMaterialSetInt(IntPtr _this, string name, int v);
        [DllImport("MeshSyncServer")] static extern void msMaterialSetFloat(IntPtr _this, string name, float v);
        [DllImport("MeshSyncServer")] static extern void msMaterialSetVector(IntPtr _this, string name, Vector4 v);
        [DllImport("MeshSyncServer")] static extern void msMaterialSetMatrix(IntPtr _this, string name, Matrix4x4 v);
        [DllImport("MeshSyncServer")] static extern void msMaterialSetFloatArray(IntPtr _this, string name, float[] v, int c);
        [DllImport("MeshSyncServer")] static extern void msMaterialSetVectorArray(IntPtr _this, string name, Vector4[] v, int c);
        [DllImport("MeshSyncServer")] static extern void msMaterialSetMatrixArray(IntPtr _this, string name, Matrix4x4[] v, int c);
        [DllImport("MeshSyncServer")] static extern void msMaterialSetTexture(IntPtr _this, string name, TextureData v);

        [DllImport("MeshSyncServer")] static extern int msMaterialGetNumKeywords(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern MaterialKeywordData msMaterialGetKeyword(IntPtr _this, int i);
        [DllImport("MeshSyncServer")] static extern void msMaterialAddKeyword(IntPtr _this, string name, byte v);
        #endregion

        public static implicit operator bool(MaterialData v)
        {
            return v._this != IntPtr.Zero;
        }

        public static MaterialData Create() { return msMaterialCreate(); }

        public int id
        {
            get { return msMaterialGetID(_this); }
            set { msMaterialSetID(_this, value); }
        }
        public string name
        {
            get { return Misc.S(msMaterialGetName(_this)); }
            set { msMaterialSetName(_this, value); }
        }
        public int index
        {
            get { return msMaterialGetIndex(_this); }
            set { msMaterialSetIndex(_this, value); }
        }
        public string shader
        {
            get { return Misc.S(msMaterialGetShader(_this)); }
            set { msMaterialSetShader(_this, value); }
        }

        public int numProperties
        {
            get { return msMaterialGetNumParams(_this); }
        }
        public MaterialPropertyData GetProperty(int i)
        {
            return msMaterialGetParam(_this, i);
        }
        public MaterialPropertyData FindProperty(string name)
        {
            return msMaterialFindParam(_this, name);
        }

        public Color color
        {
            get
            {
                var p = FindProperty("_Color");
                if (p && p.type == MaterialPropertyData.Type.Vector)
                    return p.vectorValue;
                else
                    return Color.black;
            }
            set
            {
                SetVector("_Color", value);
            }
        }

        public void SetInt(string name, int v) { msMaterialSetInt(_this, name, v); }
        public void SetFloat(string name, float v) { msMaterialSetFloat(_this, name, v); }
        public void SetVector(string name, Vector4 v) { msMaterialSetVector(_this, name, v); }
        public void SetMatrix(string name, Matrix4x4 v) { msMaterialSetMatrix(_this, name, v); }
        public void SetFloatArray(string name, float[] v) { msMaterialSetFloatArray(_this, name, v, v.Length); }
        public void SetVectorArray(string name, Vector4[] v) { msMaterialSetVectorArray(_this, name, v, v.Length); }
        public void SetMatrixArray(string name, Matrix4x4[] v) { msMaterialSetMatrixArray(_this, name, v, v.Length); }
        public void SetTexture(string name, TextureData v) { msMaterialSetTexture(_this, name, v); }

        public int numKeywords
        {
            get { return msMaterialGetNumKeywords(_this); }
        }
        public MaterialKeywordData GetKeyword(int i)
        {
            return msMaterialGetKeyword(_this, i);
        }
        public void AddKeyword(string name, bool value)
        {
            msMaterialAddKeyword(_this, name, (byte)(value ? 1 : 0));
        }
    }
    #endregion


    #region Animations
    public enum InterpolationType
    {
        Smooth,
        Linear,
        Constant,
    }
    public delegate void InterpolationMethod(AnimationCurve curve);

    public struct TransformAnimationData
    {
        #region internal
        internal IntPtr _this;

        [DllImport("MeshSyncServer")] static extern int msTransformAGetNumTranslationSamples(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern float msTransformAGetTranslationTime(IntPtr _this, int i);
        [DllImport("MeshSyncServer")] static extern Vector3 msTransformAGetTranslationValue(IntPtr _this, int i);

        [DllImport("MeshSyncServer")] static extern int msTransformAGetNumRotationSamples(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern float msTransformAGetRotationTime(IntPtr _this, int i);
        [DllImport("MeshSyncServer")] static extern Quaternion msTransformAGetRotationValue(IntPtr _this, int i);

        [DllImport("MeshSyncServer")] static extern int msTransformAGetNumScaleSamples(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern float msTransformAGetScaleTime(IntPtr _this, int i);
        [DllImport("MeshSyncServer")] static extern Vector3 msTransformAGetScaleValue(IntPtr _this, int i);

        [DllImport("MeshSyncServer")] static extern int msTransformAGetNumVisibleSamples(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern float msTransformAGetVisibleTime(IntPtr _this, int i);
        [DllImport("MeshSyncServer")] static extern byte msTransformAGetVisibleValue(IntPtr _this, int i);
        #endregion

        public static explicit operator TransformAnimationData(IntPtr v)
        {
            TransformAnimationData ret;
            ret._this = v;
            return ret;
        }
        public static implicit operator bool(TransformAnimationData v)
        {
            return v._this != IntPtr.Zero;
        }

        public AnimationCurve[] GenTranslationCurves()
        {
            int n = msTransformAGetNumTranslationSamples(_this);
            if (n == 0)
                return null;
            var x = new Keyframe[n];
            var y = new Keyframe[n];
            var z = new Keyframe[n];
            for (int i = 0; i < n; ++i)
            {
                var t = msTransformAGetTranslationTime(_this, i);
                var v = msTransformAGetTranslationValue(_this, i);
                x[i].time = y[i].time = z[i].time = t;
                x[i].value = v.x;
                y[i].value = v.y;
                z[i].value = v.z;
            }
            var ret = new AnimationCurve[] {
                    new AnimationCurve(x),
                    new AnimationCurve(y),
                    new AnimationCurve(z),
                };
            return ret;
        }

        public AnimationCurve[] GenRotationCurves()
        {
            int n = msTransformAGetNumRotationSamples(_this);
            if (n == 0)
                return null;
            var x = new Keyframe[n];
            var y = new Keyframe[n];
            var z = new Keyframe[n];
            var w = new Keyframe[n];
            for (int i = 0; i < n; ++i)
            {
                var t = msTransformAGetRotationTime(_this, i);
                var v = msTransformAGetRotationValue(_this, i);
                x[i].time = y[i].time = z[i].time = w[i].time = t;
                x[i].value = v.x;
                y[i].value = v.y;
                z[i].value = v.z;
                w[i].value = v.w;
            }
            var ret = new AnimationCurve[] {
                    new AnimationCurve(x),
                    new AnimationCurve(y),
                    new AnimationCurve(z),
                    new AnimationCurve(w),
                };
            return ret;
        }

        public AnimationCurve[] GenScaleCurves()
        {
            int n = msTransformAGetNumScaleSamples(_this);
            if (n == 0)
                return null;
            var x = new Keyframe[n];
            var y = new Keyframe[n];
            var z = new Keyframe[n];
            for (int i = 0; i < n; ++i)
            {
                var t = msTransformAGetScaleTime(_this, i);
                var v = msTransformAGetScaleValue(_this, i);
                x[i].time = y[i].time = z[i].time = t;
                x[i].value = v.x;
                y[i].value = v.y;
                z[i].value = v.z;
            }
            var ret = new AnimationCurve[] {
                    new AnimationCurve(x),
                    new AnimationCurve(y),
                    new AnimationCurve(z),
                };
            return ret;
        }

        public AnimationCurve GenVisibilityCurve()
        {
            int n = msTransformAGetNumVisibleSamples(_this);
            if (n == 0)
                return null;
            var x = new Keyframe[n];
            for (int i = 0; i < n; ++i)
            {
                var t = msTransformAGetVisibleTime(_this, i);
                var v = msTransformAGetVisibleValue(_this, i);
                x[i].time = t;
                x[i].value = v;
            }
            var ret = new AnimationCurve(x);
            return ret;
        }

#if UNITY_EDITOR
        public void ExportToClip(AnimationClip clip, GameObject root, GameObject target, string path, InterpolationMethod im)
        {
            var ttrans = typeof(Transform);
            var tgo = typeof(GameObject);

            {
                clip.SetCurve(path, ttrans, "m_LocalPosition", null);
                var curves = GenTranslationCurves();
                if (curves != null)
                {
                    foreach (var c in curves)
                        im(c);
                    clip.SetCurve(path, ttrans, "m_LocalPosition.x", curves[0]);
                    clip.SetCurve(path, ttrans, "m_LocalPosition.y", curves[1]);
                    clip.SetCurve(path, ttrans, "m_LocalPosition.z", curves[2]);
                }
            }
            {
                clip.SetCurve(path, ttrans, "m_LocalRotation", null);
                var curves = GenRotationCurves();
                if (curves != null)
                {
                    foreach (var c in curves)
                        im(c);
                    clip.SetCurve(path, ttrans, "m_LocalRotation.x", curves[0]);
                    clip.SetCurve(path, ttrans, "m_LocalRotation.y", curves[1]);
                    clip.SetCurve(path, ttrans, "m_LocalRotation.z", curves[2]);
                    clip.SetCurve(path, ttrans, "m_LocalRotation.w", curves[3]);
                }
            }
            {
                clip.SetCurve(path, ttrans, "m_LocalScale", null);
                var curves = GenScaleCurves();
                if (curves != null)
                {
                    foreach (var c in curves)
                        im(c);
                    clip.SetCurve(path, ttrans, "m_LocalScale.x", curves[0]);
                    clip.SetCurve(path, ttrans, "m_LocalScale.y", curves[1]);
                    clip.SetCurve(path, ttrans, "m_LocalScale.z", curves[2]);
                }
            }
            {
                clip.SetCurve(path, tgo, "m_IsActive", null);
                var curve = GenVisibilityCurve();
                if (curve != null)
                    im(curve);
                clip.SetCurve(path, tgo, "m_IsActive", curve);
            }
        }
#endif
    }

    public struct CameraAnimationData
    {
        #region internal
        internal IntPtr _this;

        [DllImport("MeshSyncServer")] static extern int msCameraAGetNumFovSamples(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern float msCameraAGetFovTime(IntPtr _this, int i);
        [DllImport("MeshSyncServer")] static extern float msCameraAGetFovValue(IntPtr _this, int i);

        [DllImport("MeshSyncServer")] static extern int msCameraAGetNumNearSamples(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern float msCameraAGetNearTime(IntPtr _this, int i);
        [DllImport("MeshSyncServer")] static extern float msCameraAGetNearValue(IntPtr _this, int i);

        [DllImport("MeshSyncServer")] static extern int msCameraAGetNumFarSamples(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern float msCameraAGetFarTime(IntPtr _this, int i);
        [DllImport("MeshSyncServer")] static extern float msCameraAGetFarValue(IntPtr _this, int i);

        [DllImport("MeshSyncServer")] static extern int msCameraAGetNumHApertureSamples(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern float msCameraAGetHApertureTime(IntPtr _this, int i);
        [DllImport("MeshSyncServer")] static extern float msCameraAGetHApertureValue(IntPtr _this, int i);

        [DllImport("MeshSyncServer")] static extern int msCameraAGetNumVApertureSamples(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern float msCameraAGetVApertureTime(IntPtr _this, int i);
        [DllImport("MeshSyncServer")] static extern float msCameraAGetVApertureValue(IntPtr _this, int i);

        [DllImport("MeshSyncServer")] static extern int msCameraAGetNumFocalLengthSamples(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern float msCameraAGetFocalLengthTime(IntPtr _this, int i);
        [DllImport("MeshSyncServer")] static extern float msCameraAGetFocalLengthValue(IntPtr _this, int i);

        [DllImport("MeshSyncServer")] static extern int msCameraAGetNumFocusDistanceSamples(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern float msCameraAGetFocusDistanceTime(IntPtr _this, int i);
        [DllImport("MeshSyncServer")] static extern float msCameraAGetFocusDistanceValue(IntPtr _this, int i);
        #endregion

        public static explicit operator CameraAnimationData(IntPtr v)
        {
            CameraAnimationData ret;
            ret._this = v;
            return ret;
        }
        public static implicit operator bool(CameraAnimationData v)
        {
            return v._this != IntPtr.Zero;
        }

        public AnimationCurve GenFovCurve()
        {
            int n = msCameraAGetNumFovSamples(_this);
            if (n == 0)
                return null;
            var x = new Keyframe[n];
            for (int i = 0; i < n; ++i)
            {
                var t = msCameraAGetFovTime(_this, i);
                var v = msCameraAGetFovValue(_this, i);
                x[i].time = t;
                x[i].value = v;
            }
            var ret = new AnimationCurve(x);
            return ret;
        }

        public AnimationCurve GenNearPlaneCurve()
        {
            int n = msCameraAGetNumNearSamples(_this);
            if (n == 0)
                return null;
            var x = new Keyframe[n];
            for (int i = 0; i < n; ++i)
            {
                var t = msCameraAGetNearTime(_this, i);
                var v = msCameraAGetNearValue(_this, i);
                x[i].time = t;
                x[i].value = v;
            }
            var ret = new AnimationCurve(x);
            return ret;
        }

        public AnimationCurve GenFarPlaneCurve()
        {
            int n = msCameraAGetNumFarSamples(_this);
            if (n == 0)
                return null;
            var x = new Keyframe[n];
            for (int i = 0; i < n; ++i)
            {
                var t = msCameraAGetFarTime(_this, i);
                var v = msCameraAGetFarValue(_this, i);
                x[i].time = t;
                x[i].value = v;
            }
            var ret = new AnimationCurve(x);
            return ret;
        }

#if UNITY_EDITOR
        public void ExportToClip(AnimationClip clip, GameObject root, GameObject target, string path, InterpolationMethod im)
        {
            ((TransformAnimationData)_this).ExportToClip(clip, root, target, path, im);

            var tcam = typeof(Camera);
            {
                clip.SetCurve(path, tcam, "field of view", null);
                var curve = GenFovCurve();
                if (curve != null)
                {
                    im(curve);
                    clip.SetCurve(path, tcam, "field of view", curve);
                }
            }
            {
                clip.SetCurve(path, tcam, "near clip plane", null);
                var curve = GenNearPlaneCurve();
                if (curve != null)
                {
                    im(curve);
                    clip.SetCurve(path, tcam, "near clip plane", curve);
                }
            }
            {
                clip.SetCurve(path, tcam, "far clip plane", null);
                var curve = GenFarPlaneCurve();
                if (curve != null)
                {
                    im(curve);
                    clip.SetCurve(path, tcam, "far clip plane", curve);
                }
            }
        }
#endif
    }

    public struct LightAnimationData
    {
        #region internal
        internal IntPtr _this;

        [DllImport("MeshSyncServer")] static extern int msLightAGetNumColorSamples(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern float msLightAGetColorTime(IntPtr _this, int i);
        [DllImport("MeshSyncServer")] static extern Color msLightAGetColorValue(IntPtr _this, int i);

        [DllImport("MeshSyncServer")] static extern int msLightAGetNumIntensitySamples(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern float msLightAGetIntensityTime(IntPtr _this, int i);
        [DllImport("MeshSyncServer")] static extern float msLightAGetIntensityValue(IntPtr _this, int i);

        [DllImport("MeshSyncServer")] static extern int msLightAGetNumRangeSamples(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern float msLightAGetRangeTime(IntPtr _this, int i);
        [DllImport("MeshSyncServer")] static extern float msLightAGetRangeValue(IntPtr _this, int i);

        [DllImport("MeshSyncServer")] static extern int msLightAGetNumSpotAngleSamples(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern float msLightAGetSpotAngleTime(IntPtr _this, int i);
        [DllImport("MeshSyncServer")] static extern float msLightAGetSpotAngleValue(IntPtr _this, int i);
        #endregion


        public static explicit operator LightAnimationData(IntPtr v)
        {
            LightAnimationData ret;
            ret._this = v;
            return ret;
        }
        public static implicit operator bool(LightAnimationData v)
        {
            return v._this != IntPtr.Zero;
        }

        public AnimationCurve[] GenColorCurves()
        {
            int n = msLightAGetNumColorSamples(_this);
            if (n == 0)
                return null;
            var x = new Keyframe[n];
            var y = new Keyframe[n];
            var z = new Keyframe[n];
            var w = new Keyframe[n];
            for (int i = 0; i < n; ++i)
            {
                var t = msLightAGetColorTime(_this, i);
                var v = msLightAGetColorValue(_this, i);
                x[i].time = y[i].time = z[i].time = w[i].time = t;
                x[i].value = v.r;
                y[i].value = v.g;
                z[i].value = v.b;
                w[i].value = v.a;
            }
            var ret = new AnimationCurve[] {
                    new AnimationCurve(x),
                    new AnimationCurve(y),
                    new AnimationCurve(z),
                    new AnimationCurve(w),
                };
            return ret;
        }

        public AnimationCurve GenIntensityCurve()
        {
            int n = msLightAGetNumIntensitySamples(_this);
            if (n == 0)
                return null;
            var x = new Keyframe[n];
            for (int i = 0; i < n; ++i)
            {
                var t = msLightAGetIntensityTime(_this, i);
                var v = msLightAGetIntensityValue(_this, i);
                x[i].time = t;
                x[i].value = v;
            }
            var ret = new AnimationCurve(x);
            return ret;
        }

        public AnimationCurve GenRangeCurve()
        {
            int n = msLightAGetNumRangeSamples(_this);
            if (n == 0)
                return null;
            var x = new Keyframe[n];
            for (int i = 0; i < n; ++i)
            {
                var t = msLightAGetRangeTime(_this, i);
                var v = msLightAGetRangeValue(_this, i);
                x[i].time = t;
                x[i].value = v;
            }
            var ret = new AnimationCurve(x);
            return ret;
        }

        public AnimationCurve GenSpotAngleCurve()
        {
            int n = msLightAGetNumSpotAngleSamples(_this);
            if (n == 0)
                return null;
            var x = new Keyframe[n];
            for (int i = 0; i < n; ++i)
            {
                var t = msLightAGetSpotAngleTime(_this, i);
                var v = msLightAGetSpotAngleValue(_this, i);
                x[i].time = t;
                x[i].value = v;
            }
            var ret = new AnimationCurve(x);
            return ret;
        }

#if UNITY_EDITOR
        public void ExportToClip(AnimationClip clip, GameObject root, GameObject target, string path, InterpolationMethod im)
        {
            ((TransformAnimationData)_this).ExportToClip(clip, root, target, path, im);

            var tlight = typeof(Light);
            {
                clip.SetCurve(path, tlight, "m_Color", null);
                var curves = GenColorCurves();
                if (curves != null)
                {
                    foreach (var c in curves)
                        im(c);
                    clip.SetCurve(path, tlight, "m_Color.r", curves[0]);
                    clip.SetCurve(path, tlight, "m_Color.g", curves[1]);
                    clip.SetCurve(path, tlight, "m_Color.b", curves[2]);
                    clip.SetCurve(path, tlight, "m_Color.a", curves[3]);
                }
            }
            {
                clip.SetCurve(path, tlight, "m_Intensity", null);
                var curve = GenIntensityCurve();
                if (curve != null)
                {
                    im(curve);
                    clip.SetCurve(path, tlight, "m_Intensity", curve);
                }
            }
            {
                clip.SetCurve(path, tlight, "m_Range", null);
                var curve = GenRangeCurve();
                if (curve != null)
                {
                    im(curve);
                    clip.SetCurve(path, tlight, "m_Range", curve);
                }
            }
            {
                clip.SetCurve(path, tlight, "m_SpotAngle", null);
                var curve = GenSpotAngleCurve();
                if (curve != null)
                {
                    im(curve);
                    clip.SetCurve(path, tlight, "m_SpotAngle", curve);
                }
            }
        }
#endif
    }

    public struct MeshAnimationData
    {
        #region internal
        internal IntPtr _this;

        [DllImport("MeshSyncServer")] static extern int msMeshAGetNumBlendshapes(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern IntPtr msMeshAGetBlendshapeName(IntPtr _this, int bi);
        [DllImport("MeshSyncServer")] static extern int msMeshAGetNumBlendshapeSamples(IntPtr _this, int bi);
        [DllImport("MeshSyncServer")] static extern float msMeshAGetNumBlendshapeTime(IntPtr _this, int bi, int si);
        [DllImport("MeshSyncServer")] static extern float msMeshAGetNumBlendshapeWeight(IntPtr _this, int bi, int si);
        #endregion


        public static explicit operator MeshAnimationData(IntPtr v)
        {
            MeshAnimationData ret;
            ret._this = v;
            return ret;
        }
        public static implicit operator bool(MeshAnimationData v)
        {
            return v._this != IntPtr.Zero;
        }

#if UNITY_EDITOR
        public void ExportToClip(AnimationClip clip, GameObject root, GameObject target, string path, InterpolationMethod im)
        {
            ((TransformAnimationData)_this).ExportToClip(clip, root, target, path, im);

            var tsmr = typeof(SkinnedMeshRenderer);
            {
                // blendshape animation

                int numBS = msMeshAGetNumBlendshapes(_this);
                for (int bi = 0; bi < numBS; ++bi)
                {
                    string name = "blendShape." + Misc.S(msMeshAGetBlendshapeName(_this, bi));
                    clip.SetCurve(path, tsmr, name, null);

                    int numKeyframes = msMeshAGetNumBlendshapeSamples(_this, bi);
                    if (numKeyframes > 0)
                    {
                        var kf = new Keyframe[numKeyframes];
                        for (int ki = 0; ki < numKeyframes; ++ki)
                        {
                            kf[ki].time = msMeshAGetNumBlendshapeTime(_this, bi, ki);
                            kf[ki].value = msMeshAGetNumBlendshapeWeight(_this, bi, ki);
                        }

                        var curve = new AnimationCurve(kf);
                        im(curve);
                        clip.SetCurve(path, tsmr, name, curve);
                    }
                }
            }
        }
#endif
    }

    public struct PointsAnimationData
    {
        #region internal
        internal IntPtr _this;

        [DllImport("MeshSyncServer")] static extern int msPointsAGetNumTimeSamples(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern float msPointsAGetTimeTime(IntPtr _this, int i);
        [DllImport("MeshSyncServer")] static extern float msPointsAGetTimeValue(IntPtr _this, int i);
        #endregion


        public static explicit operator PointsAnimationData(IntPtr v)
        {
            PointsAnimationData ret;
            ret._this = v;
            return ret;
        }
        public static implicit operator bool(PointsAnimationData v)
        {
            return v._this != IntPtr.Zero;
        }

        public AnimationCurve GenTimeCurve()
        {
            int n = msPointsAGetNumTimeSamples(_this);
            if (n == 0)
                return null;
            var x = new Keyframe[n];
            for (int i = 0; i < n; ++i)
            {
                var t = msPointsAGetTimeTime(_this, i);
                var v = msPointsAGetTimeValue(_this, i);
                x[i].time = t;
                x[i].value = v;
            }
            var ret = new AnimationCurve(x);
            return ret;
        }

#if UNITY_EDITOR
        public void ExportToClip(AnimationClip clip, GameObject root, GameObject target, string path, InterpolationMethod im)
        {
            ((TransformAnimationData)_this).ExportToClip(clip, root, target, path, im);

            var tpoints = typeof(Points);
            {
                clip.SetCurve(path, tpoints, "m_time", null);
                var curve = GenTimeCurve();
                if (curve != null)
                {
                    im(curve);
                    clip.SetCurve(path, tpoints, "m_time", curve);
                }
            }
        }
#endif
    }

    public struct AnimationData
    {
        #region internal
        internal IntPtr _this;
        [DllImport("MeshSyncServer")] static extern IntPtr msAnimationGetPath(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern TransformData.Type msAnimationGetType(IntPtr _this);
        #endregion


        public static explicit operator AnimationData(IntPtr v)
        {
            AnimationData ret;
            ret._this = v;
            return ret;
        }
        public static implicit operator bool(AnimationData v)
        {
            return v._this != IntPtr.Zero;
        }

        public string path
        {
            get { return Misc.S(msAnimationGetPath(_this)); }
        }

        public TransformData.Type type
        {
            get { return msAnimationGetType(_this); }
        }

#if UNITY_EDITOR
        public static void SmoothInterpolation(AnimationCurve curve)
        {
            int len = curve.length;
            for (int i = 0; i < len; ++i)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
            }
        }

        public static void LinearInterpolation(AnimationCurve curve)
        {
            int len = curve.length;
            for (int i = 0; i < len; ++i)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
            }
        }

        public static void ConstantInterpolation(AnimationCurve curve)
        {
            int len = curve.length;
            for (int i = 0; i < len; ++i)
            {
                AnimationUtility.SetKeyBroken(curve, i, true);
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Constant);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Constant);
            }
        }

        public void ExportToClip(AnimationClip clip, GameObject root, GameObject target, string path, InterpolationMethod im)
        {
            switch (type)
            {
                case TransformData.Type.Transform:
                    ((TransformAnimationData)_this).ExportToClip(clip, root, target, path, im);
                    break;
                case TransformData.Type.Camera:
                    ((CameraAnimationData)_this).ExportToClip(clip, root, target, path, im);
                    break;
                case TransformData.Type.Light:
                    ((LightAnimationData)_this).ExportToClip(clip, root, target, path, im);
                    break;
                case TransformData.Type.Mesh:
                    ((MeshAnimationData)_this).ExportToClip(clip, root, target, path, im);
                    break;
                case TransformData.Type.Points:
                    ((PointsAnimationData)_this).ExportToClip(clip, root, target, path, im);
                    break;
            }
        }
#endif
    }

    public struct AnimationClipData
    {
        #region internal
        internal IntPtr _this;
        [DllImport("MeshSyncServer")] static extern IntPtr msAnimationClipGetName(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern int msAnimationClipGetNumAnimations(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern AnimationData msAnimationClipGetAnimationData(IntPtr _this, int i);
        #endregion

        public string name
        {
            get { return Misc.S(msAnimationClipGetName(_this)); }
        }
        public int numAnimations
        {
            get { return msAnimationClipGetNumAnimations(_this); }
        }
        public AnimationData GetAnimation(int i)
        {
            return msAnimationClipGetAnimationData(_this, i);
        }
    }
    #endregion


    #region Entities
    public struct Identifier
    {
        #region internal
        internal IntPtr _this;
        [DllImport("MeshSyncServer")] static extern int msIdentifierGetID(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern IntPtr msIdentifierGetName(IntPtr _this);
        #endregion

        public int id { get { return msIdentifierGetID(_this); } }
        public string name { get { return Misc.S(msIdentifierGetName(_this)); } }
    }

    public struct TransformData
    {
        #region internal
        internal IntPtr _this;
        [DllImport("MeshSyncServer")] static extern TransformData msTransformCreate();
        [DllImport("MeshSyncServer")] static extern Type msTransformGetType(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern int msTransformGetID(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern void msTransformSetID(IntPtr _this, int v);
        [DllImport("MeshSyncServer")] static extern int msTransformGetIndex(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern void msTransformSetIndex(IntPtr _this, int v);
        [DllImport("MeshSyncServer")] static extern IntPtr msTransformGetPath(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern void msTransformSetPath(IntPtr _this, string v);
        [DllImport("MeshSyncServer")] static extern Vector3 msTransformGetPosition(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern void msTransformSetPosition(IntPtr _this, Vector3 v);
        [DllImport("MeshSyncServer")] static extern Quaternion msTransformGetRotation(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern void msTransformSetRotation(IntPtr _this, Quaternion v);
        [DllImport("MeshSyncServer")] static extern Vector3 msTransformGetScale(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern void msTransformSetScale(IntPtr _this, Vector3 v);
        [DllImport("MeshSyncServer")] static extern byte msTransformGetVisible(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern void msTransformSetVisible(IntPtr _this, byte v);
        [DllImport("MeshSyncServer")] static extern byte msTransformGetVisibleHierarchy(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern void msTransformSetVisibleHierarchy(IntPtr _this, byte v);
        [DllImport("MeshSyncServer")] static extern IntPtr msTransformGetReference(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern void msTransformSetReference(IntPtr _this, string v);
        #endregion

        public enum Type
        {
            Unknown,
            Transform,
            Camera,
            Light,
            Mesh,
            Points,
        };

        public static explicit operator TransformData(IntPtr v)
        {
            TransformData ret;
            ret._this = v;
            return ret;
        }

        public static TransformData Create()
        {
            return msTransformCreate();
        }

        public Type type
        {
            get { return msTransformGetType(_this); }
        }
        public int id
        {
            get { return msTransformGetID(_this); }
            set { msTransformSetID(_this, value); }
        }
        public int index
        {
            get { return msTransformGetIndex(_this); }
            set { msTransformSetIndex(_this, value); }
        }
        public string path
        {
            get { return Misc.S(msTransformGetPath(_this)); }
            set { msTransformSetPath(_this, value); }
        }
        public Vector3 position
        {
            get { return msTransformGetPosition(_this); }
            set { msTransformSetPosition(_this, value); }
        }
        public Quaternion rotation
        {
            get { return msTransformGetRotation(_this); }
            set { msTransformSetRotation(_this, value); }
        }
        public Vector3 scale
        {
            get { return msTransformGetScale(_this); }
            set { msTransformSetScale(_this, value); }
        }
        public bool visible
        {
            get { return msTransformGetVisible(_this) != 0; }
            set { msTransformSetVisible(_this, (byte)(value ? 1 : 0)); }
        }
        public bool visibleHierarchy
        {
            get { return msTransformGetVisibleHierarchy(_this) != 0; }
            set { msTransformSetVisibleHierarchy(_this, (byte)(value ? 1 : 0)); }
        }
        public string reference
        {
            get { return Misc.S(msTransformGetReference(_this)); }
            set { msTransformSetReference(_this, value); }
        }
    }

    public struct CameraData
    {
        #region internal
        internal IntPtr _this;
        [DllImport("MeshSyncServer")] static extern CameraData msCameraCreate();
        [DllImport("MeshSyncServer")] static extern byte msCameraIsOrtho(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern void msCameraSetOrtho(IntPtr _this, byte v);
        [DllImport("MeshSyncServer")] static extern float msCameraGetFov(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern void msCameraSetFov(IntPtr _this, float v);
        [DllImport("MeshSyncServer")] static extern float msCameraGetNearPlane(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern void msCameraSetNearPlane(IntPtr _this, float v);
        [DllImport("MeshSyncServer")] static extern float msCameraGetFarPlane(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern void msCameraSetFarPlane(IntPtr _this, float v);
        [DllImport("MeshSyncServer")] static extern float msCameraGetHorizontalAperture(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern void msCameraSetHorizontalAperture(IntPtr _this, float v);
        [DllImport("MeshSyncServer")] static extern float msCameraGetVerticalAperture(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern void msCameraSetVerticalAperture(IntPtr _this, float v);
        [DllImport("MeshSyncServer")] static extern float msCameraGetFocalLength(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern void msCameraSetFocalLength(IntPtr _this, float v);
        [DllImport("MeshSyncServer")] static extern float msCameraGetFocusDistance(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern void msCameraSetFocusDistance(IntPtr _this, float v);
        #endregion


        public static explicit operator CameraData(IntPtr v)
        {
            CameraData ret;
            ret._this = v;
            return ret;
        }

        public static CameraData Create()
        {
            return msCameraCreate();
        }

        public TransformData transform
        {
            get { return (TransformData)_this; }
        }

        public bool orthographic
        {
            get { return msCameraIsOrtho(_this) != 0; }
            set { msCameraSetOrtho(_this, (byte)(value ? 1 : 0)); }
        }
        public float fov
        {
            get { return msCameraGetFov(_this); }
            set { msCameraSetFov(_this, value); }
        }
        public float nearClipPlane
        {
            get { return msCameraGetNearPlane(_this); }
            set { msCameraSetNearPlane(_this, value); }
        }
        public float farClipPlane
        {
            get { return msCameraGetFarPlane(_this); }
            set { msCameraSetFarPlane(_this, value); }
        }
        public float horizontalAperture
        {
            get { return msCameraGetHorizontalAperture(_this); }
            set { msCameraSetHorizontalAperture(_this, value); }
        }
        public float verticalAperture
        {
            get { return msCameraGetVerticalAperture(_this); }
            set { msCameraSetVerticalAperture(_this, value); }
        }
        public float focalLength
        {
            get { return msCameraGetFocalLength(_this); }
            set { msCameraSetFocalLength(_this, value); }
        }
        public float focusDistance
        {
            get { return msCameraGetFocusDistance(_this); }
            set { msCameraSetFocusDistance(_this, value); }
        }
    }

    public struct LightData
    {
        #region internal
        internal IntPtr _this;

        [DllImport("MeshSyncServer")] static extern LightData msLightCreate();
        [DllImport("MeshSyncServer")] static extern LightType msLightGetType(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern void msLightSetType(IntPtr _this, LightType v);
        [DllImport("MeshSyncServer")] static extern Color msLightGetColor(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern void msLightSetColor(IntPtr _this, Color v);
        [DllImport("MeshSyncServer")] static extern float msLightGetIntensity(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern void msLightSetIntensity(IntPtr _this, float v);
        [DllImport("MeshSyncServer")] static extern float msLightGetRange(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern void msLightSetRange(IntPtr _this, float v);
        [DllImport("MeshSyncServer")] static extern float msLightGetSpotAngle(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern void msLightSetSpotAngle(IntPtr _this, float v);
        #endregion

        public static explicit operator LightData(IntPtr v)
        {
            LightData ret;
            ret._this = v;
            return ret;
        }

        public static LightData Create()
        {
            return msLightCreate();
        }

        public TransformData transform
        {
            get { return (TransformData)_this; }
        }

        public LightType type
        {
            get { return msLightGetType(_this); }
            set { msLightSetType(_this, value); }
        }
        public Color color
        {
            get { return msLightGetColor(_this); }
            set { msLightSetColor(_this, value); }
        }
        public float intensity
        {
            get { return msLightGetIntensity(_this); }
            set { msLightSetIntensity(_this, value); }
        }
        public float range
        {
            get { return msLightGetRange(_this); }
            set { msLightSetRange(_this, value); }
        }
        public float spotAngle
        {
            get { return msLightGetSpotAngle(_this); }
            set { msLightSetSpotAngle(_this, value); }
        }
    }

    #region Mesh
    public struct SubmeshData
    {
        #region internal
        internal IntPtr _this;
        [DllImport("MeshSyncServer")] static extern int msSubmeshGetNumIndices(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern void msSubmeshReadIndices(IntPtr _this, IntPtr dst);
        [DllImport("MeshSyncServer")] static extern int msSubmeshGetMaterialID(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern Topology msSubmeshGetTopology(IntPtr _this);
        #endregion

        public enum Topology
        {
            Points,
            Lines,
            Triangles,
            Quads,
        };


        public int numIndices { get { return msSubmeshGetNumIndices(_this); } }
        public Topology topology { get { return msSubmeshGetTopology(_this); } }
        public int materialID { get { return msSubmeshGetMaterialID(_this); } }
        public void ReadIndices(PinnedList<int> dst) { msSubmeshReadIndices(_this, dst); }
    }

    public struct SplitData
    {
        #region internal
        internal IntPtr _this;
        [DllImport("MeshSyncServer")] static extern int msSplitGetNumPoints(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern int msSplitGetNumIndices(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern Vector3 msSplitGetBoundsCenter(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern Vector3 msSplitGetBoundsSize(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern int msSplitGetNumSubmeshes(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern SubmeshData msSplitGetSubmesh(IntPtr _this, int i);
        #endregion

        public int numPoints { get { return msSplitGetNumPoints(_this); } }
        public int numIndices { get { return msSplitGetNumIndices(_this); } }
        public Bounds bounds { get { return new Bounds(msSplitGetBoundsCenter(_this), msSplitGetBoundsSize(_this)); } }
        public int numSubmeshes { get { return msSplitGetNumSubmeshes(_this); } }

        public SubmeshData GetSubmesh(int i)
        {
            return msSplitGetSubmesh(_this, i);
        }
    }

    public struct BlendShapeData
    {
        #region internal
        internal IntPtr _this;
        [DllImport("MeshSyncServer")] static extern IntPtr msBlendShapeGetName(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern float msBlendShapeGetWeight(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern int msBlendShapeGetNumFrames(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern float msBlendShapeGetFrameWeight(IntPtr _this, int f);
        [DllImport("MeshSyncServer")] static extern void msBlendShapeReadPoints(IntPtr _this, int f, IntPtr dst, SplitData split);
        [DllImport("MeshSyncServer")] static extern void msBlendShapeReadNormals(IntPtr _this, int f, IntPtr dst, SplitData split);
        [DllImport("MeshSyncServer")] static extern void msBlendShapeReadTangents(IntPtr _this, int f, IntPtr dst, SplitData split);
        [DllImport("MeshSyncServer")] static extern void msBlendShapeAddFrame(IntPtr _this, float weight, int num, Vector3[] v, Vector3[] n, Vector3[] t);
        #endregion

        public string name
        {
            get { return Misc.S(msBlendShapeGetName(_this)); }
        }
        public float weight
        {
            get { return msBlendShapeGetWeight(_this); }
        }
        public float numFrames
        {
            get { return msBlendShapeGetNumFrames(_this); }
        }
        public float GetWeight(int f) { return msBlendShapeGetFrameWeight(_this, f); }
        public void ReadPoints(int f, PinnedList<Vector3> dst, SplitData split) { msBlendShapeReadPoints(_this, f, dst, split); }
        public void ReadNormals(int f, PinnedList<Vector3> dst, SplitData split) { msBlendShapeReadNormals(_this, f, dst, split); }
        public void ReadTangents(int f, PinnedList<Vector3> dst, SplitData split) { msBlendShapeReadTangents(_this, f, dst, split); }

        public void AddFrame(float w, Vector3[] v, Vector3[] n, Vector3[] t)
        {
            msBlendShapeAddFrame(_this, w, v.Length, v, n, t);
        }
    }

    public struct MeshDataFlags
    {
        public BitFlags flags;
        public bool hasRefineSettings
        {
            get { return flags[0]; }
            set { flags[0] = value; }
        }
        public bool hasIndices
        {
            get { return flags[1]; }
            set { flags[1] = value; }
        }
        public bool hasCounts
        {
            get { return flags[2]; }
            set { flags[2] = value; }
        }
        public bool hasPoints
        {
            get { return flags[3]; }
            set { flags[3] = value; }
        }
        public bool hasNormals
        {
            get { return flags[4]; }
            set { flags[4] = value; }
        }
        public bool hasTangents
        {
            get { return flags[5]; }
            set { flags[5] = value; }
        }
        public bool hasUV0
        {
            get { return flags[6]; }
            set { flags[6] = value; }
        }
        public bool hasUV1
        {
            get { return flags[7]; }
            set { flags[7] = value; }
        }
        public bool hasColors
        {
            get { return flags[8]; }
            set { flags[8] = value; }
        }
        public bool hasMaterialIDs
        {
            get { return flags[9]; }
            set { flags[9] = value; }
        }
        public bool hasBones
        {
            get { return flags[10]; }
            set { flags[10] = value; }
        }
        public bool hasBlendshapeWeights
        {
            get { return flags[11]; }
            set { flags[11] = value; }
        }
        public bool hasBlendshapes
        {
            get { return flags[12]; }
            set { flags[12] = value; }
        }
        public bool applyTRS
        {
            get { return flags[13]; }
            set { flags[13] = value; }
        }
    };

    public struct MeshData
    {
        #region internal
        internal IntPtr _this;

        [DllImport("MeshSyncServer")] static extern MeshData msMeshCreate();
        [DllImport("MeshSyncServer")] static extern MeshDataFlags msMeshGetFlags(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern void msMeshSetFlags(IntPtr _this, MeshDataFlags v);
        [DllImport("MeshSyncServer")] static extern int msMeshGetNumPoints(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern int msMeshGetNumIndices(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern int msMeshGetNumSplits(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern void msMeshReadPoints(IntPtr _this, IntPtr dst, SplitData split);
        [DllImport("MeshSyncServer")] static extern void msMeshWritePoints(IntPtr _this, Vector3[] v, int size);
        [DllImport("MeshSyncServer")] static extern void msMeshReadNormals(IntPtr _this, IntPtr dst, SplitData split);
        [DllImport("MeshSyncServer")] static extern void msMeshWriteNormals(IntPtr _this, Vector3[] v, int size);
        [DllImport("MeshSyncServer")] static extern void msMeshReadTangents(IntPtr _this, IntPtr dst, SplitData split);
        [DllImport("MeshSyncServer")] static extern void msMeshWriteTangents(IntPtr _this, Vector4[] v, int size);
        [DllImport("MeshSyncServer")] static extern void msMeshReadUV0(IntPtr _this, IntPtr dst, SplitData split);
        [DllImport("MeshSyncServer")] static extern void msMeshReadUV1(IntPtr _this, IntPtr dst, SplitData split);
        [DllImport("MeshSyncServer")] static extern void msMeshWriteUV0(IntPtr _this, Vector2[] v, int size);
        [DllImport("MeshSyncServer")] static extern void msMeshWriteUV1(IntPtr _this, Vector2[] v, int size);
        [DllImport("MeshSyncServer")] static extern void msMeshReadColors(IntPtr _this, IntPtr dst, SplitData split);
        [DllImport("MeshSyncServer")] static extern void msMeshWriteColors(IntPtr _this, Color[] v, int size);
        [DllImport("MeshSyncServer")] static extern void msMeshReadWeights4(IntPtr _this, IntPtr dst, SplitData split);
        [DllImport("MeshSyncServer")] static extern void msMeshWriteWeights4(IntPtr _this, BoneWeight[] weights, int size);
        [DllImport("MeshSyncServer")] static extern void msMeshReadIndices(IntPtr _this, IntPtr dst, SplitData split);
        [DllImport("MeshSyncServer")] static extern void msMeshWriteIndices(IntPtr _this, int[] v, int size);
        [DllImport("MeshSyncServer")] static extern void msMeshWriteSubmeshTriangles(IntPtr _this, int[] v, int size, int materialID);

        [DllImport("MeshSyncServer")] static extern int msMeshGetNumBones(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern IntPtr msMeshGetRootBonePath(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern void msMeshSetRootBonePath(IntPtr _this, string v);
        [DllImport("MeshSyncServer")] static extern IntPtr msMeshGetBonePath(IntPtr _this, int i);
        [DllImport("MeshSyncServer")] static extern void msMeshSetBonePath(IntPtr _this, string v, int i);
        [DllImport("MeshSyncServer")] static extern void msMeshReadBindPoses(IntPtr _this, Matrix4x4[] v);
        [DllImport("MeshSyncServer")] static extern void msMeshWriteBindPoses(IntPtr _this, Matrix4x4[] v, int size);

        [DllImport("MeshSyncServer")] static extern void msMeshSetLocal2World(IntPtr _this, ref Matrix4x4 v);
        [DllImport("MeshSyncServer")] static extern void msMeshSetWorld2Local(IntPtr _this, ref Matrix4x4 v);

        [DllImport("MeshSyncServer")] static extern SplitData msMeshGetSplit(IntPtr _this, int i);
        [DllImport("MeshSyncServer")] static extern int msMeshGetNumSubmeshes(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern SubmeshData msMeshGetSubmesh(IntPtr _this, int i);

        [DllImport("MeshSyncServer")] static extern int msMeshGetNumBlendShapes(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern BlendShapeData msMeshGetBlendShapeData(IntPtr _this, int i);
        [DllImport("MeshSyncServer")] static extern BlendShapeData msMeshAddBlendShape(IntPtr _this, string name);
        #endregion

        public static MeshData Create()
        {
            return msMeshCreate();
        }

        public static explicit operator MeshData(IntPtr v)
        {
            MeshData ret;
            ret._this = v;
            return ret;
        }

        public TransformData transform
        {
            get { return (TransformData)_this; }
        }

        public MeshDataFlags flags
        {
            get { return msMeshGetFlags(_this); }
            set { msMeshSetFlags(_this, value); }
        }

        public int numPoints { get { return msMeshGetNumPoints(_this); } }
        public int numIndices { get { return msMeshGetNumIndices(_this); } }
        public int numSplits { get { return msMeshGetNumSplits(_this); } }

        public void ReadPoints(PinnedList<Vector3> dst, SplitData split) { msMeshReadPoints(_this, dst, split); }
        public void ReadNormals(PinnedList<Vector3> dst, SplitData split) { msMeshReadNormals(_this, dst, split); }
        public void ReadTangents(PinnedList<Vector4> dst, SplitData split) { msMeshReadTangents(_this, dst, split); }
        public void ReadUV0(PinnedList<Vector2> dst, SplitData split) { msMeshReadUV0(_this, dst, split); }
        public void ReadUV1(PinnedList<Vector2> dst, SplitData split) { msMeshReadUV1(_this, dst, split); }
        public void ReadColors(PinnedList<Color> dst, SplitData split) { msMeshReadColors(_this, dst, split); }
        public void ReadBoneWeights(IntPtr dst, SplitData split) { msMeshReadWeights4(_this, dst, split); }
        public void ReadIndices(IntPtr dst, SplitData split) { msMeshReadIndices(_this, dst, split); }

        public void WritePoints(Vector3[] v) { msMeshWritePoints(_this, v, v.Length); }
        public void WriteNormals(Vector3[] v) { msMeshWriteNormals(_this, v, v.Length); }
        public void WriteTangents(Vector4[] v) { msMeshWriteTangents(_this, v, v.Length); }
        public void WriteUV0(Vector2[] v) { msMeshWriteUV0(_this, v, v.Length); }
        public void WriteUV1(Vector2[] v) { msMeshWriteUV1(_this, v, v.Length); }
        public void WriteColors(Color[] v) { msMeshWriteColors(_this, v, v.Length); }
        public void WriteWeights(BoneWeight[] v) { msMeshWriteWeights4(_this, v, v.Length); }
        public void WriteIndices(int[] v) { msMeshWriteIndices(_this, v, v.Length); }

        public Matrix4x4 local2world { set { msMeshSetLocal2World(_this, ref value); } }
        public Matrix4x4 world2local { set { msMeshSetWorld2Local(_this, ref value); } }

        public SplitData GetSplit(int i) { return msMeshGetSplit(_this, i); }
        public void WriteSubmeshTriangles(int[] indices, int materialID)
        {
            msMeshWriteSubmeshTriangles(_this, indices, indices.Length, materialID);
        }

        public int numBones
        {
            get { return msMeshGetNumBones(_this); }
        }
        public string rootBonePath
        {
            get { return Misc.S(msMeshGetRootBonePath(_this)); }
            set { msMeshSetRootBonePath(_this, value); }
        }
        public Matrix4x4[] bindposes
        {
            get
            {
                var ret = new Matrix4x4[numBones];
                msMeshReadBindPoses(_this, ret);
                return ret;
            }
            set { msMeshWriteBindPoses(_this, value, value.Length); }
        }
        public void SetBonePaths(MeshSyncServer mss, Transform[] bones)
        {
            int n = bones.Length;
            for (int i = 0; i < n; ++i)
            {
                string path = mss.BuildPath(bones[i]);
                msMeshSetBonePath(_this, path, i);
            }
        }
        public string[] GetBonePaths()
        {
            int n = numBones;
            var ret = new string[n];
            for (int i = 0; i < n; ++i)
                ret[i] = Misc.S(msMeshGetBonePath(_this, i));
            return ret;
        }

        public int numSubmeshes { get { return msMeshGetNumSubmeshes(_this); } }
        public SubmeshData GetSubmesh(int i)
        {
            return msMeshGetSubmesh(_this, i);
        }

        public int numBlendShapes { get { return msMeshGetNumBlendShapes(_this); } }
        public BlendShapeData GetBlendShapeData(int i)
        {
            return msMeshGetBlendShapeData(_this, i);
        }
        public BlendShapeData AddBlendShape(string name)
        {
            return msMeshAddBlendShape(_this, name);
        }
    };
    #endregion

    #region Point
    public struct PointsDataFlags
    {
        public BitFlags flags;
        public bool hasPoints
        {
            get { return flags[0]; }
            set { flags[0] = value; }
        }
        public bool hasRotations
        {
            get { return flags[1]; }
            set { flags[1] = value; }
        }
        public bool hasScales
        {
            get { return flags[2]; }
            set { flags[2] = value; }
        }
        public bool hasVelocities
        {
            get { return flags[3]; }
            set { flags[3] = value; }
        }
        public bool hasColors
        {
            get { return flags[4]; }
            set { flags[4] = value; }
        }
        public bool hasIDs
        {
            get { return flags[5]; }
            set { flags[5] = value; }
        }
    };

    public struct PointsCacheData
    {
        #region internal
        internal IntPtr _this;

        [DllImport("MeshSyncServer")] static extern PointsDataFlags msPointsDataGetFlags(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern float msPointsDataGetTime(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern void msPointsDataSetTime(IntPtr _this, float v);
        [DllImport("MeshSyncServer")] static extern void msPointsDataGetBounds(IntPtr _this, ref Vector3 center, ref Vector3 extents);
        [DllImport("MeshSyncServer")] static extern int msPointsDataGetNumPoints(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern void msPointsDataReadPoints(IntPtr _this, Vector3[] dst);
        [DllImport("MeshSyncServer")] static extern void msPointsDataWritePoints(IntPtr _this, Vector3[] v, int size);
        [DllImport("MeshSyncServer")] static extern void msPointsDataReadRotations(IntPtr _this, Quaternion[] dst);
        [DllImport("MeshSyncServer")] static extern void msPointsDataWriteRotations(IntPtr _this, Quaternion[] v, int size);
        [DllImport("MeshSyncServer")] static extern void msPointsDataReadScales(IntPtr _this, Vector3[] dst);
        [DllImport("MeshSyncServer")] static extern void msPointsDataWriteScales(IntPtr _this, Vector3[] v, int size);
        [DllImport("MeshSyncServer")] static extern void msPointsDataReadVelocities(IntPtr _this, Vector3[] dst);
        [DllImport("MeshSyncServer")] static extern void msPointsDataWriteVelocities(IntPtr _this, Vector3[] v, int size);
        [DllImport("MeshSyncServer")] static extern void msPointsDataReadColors(IntPtr _this, Color[] dst);
        [DllImport("MeshSyncServer")] static extern void msPointsDataWriteColors(IntPtr _this, Color[] v, int size);
        [DllImport("MeshSyncServer")] static extern void msPointsDataReadIDs(IntPtr _this, int[] dst);
        [DllImport("MeshSyncServer")] static extern void msPointsDataWriteIDs(IntPtr _this, int[] v, int size);
        #endregion

        public PointsDataFlags flags
        {
            get { return msPointsDataGetFlags(_this); }
        }
        public float time
        {
            get { return msPointsDataGetTime(_this); }
            set { msPointsDataSetTime(_this, value); }
        }
        public Bounds bounds
        {
            get
            {
                Vector3 c = default(Vector3);
                Vector3 e = default(Vector3);
                msPointsDataGetBounds(_this, ref c, ref e);
                return new Bounds(c, e);
            }
        }
        public int numPoints { get { return msPointsDataGetNumPoints(_this); } }

        public void ReadPoints(Vector3[] dst) { msPointsDataReadPoints(_this, dst); }
        public void ReadRotations(Quaternion[] dst) { msPointsDataReadRotations(_this, dst); }
        public void ReadScales(Vector3[] dst) { msPointsDataReadScales(_this, dst); }
        public void ReadVelocities(Vector3[] dst) { msPointsDataReadVelocities(_this, dst); }
        public void ReadColors(Color[] dst) { msPointsDataReadColors(_this, dst); }
        public void ReadIDs(int[] dst) { msPointsDataReadIDs(_this, dst); }

        public void WritePoints(Vector3[] v) { msPointsDataWritePoints(_this, v, v.Length); }
        public void WriteRotations(Quaternion[] v) { msPointsDataWriteRotations(_this, v, v.Length); }
        public void WriteScales(Vector3[] v) { msPointsDataWriteScales(_this, v, v.Length); }
        public void WriteVelocities(Vector3[] v) { msPointsDataWriteVelocities(_this, v, v.Length); }
        public void WriteColors(Color[] v) { msPointsDataWriteColors(_this, v, v.Length); }
        public void WriteIDs(int[] v) { msPointsDataWriteIDs(_this, v, v.Length); }
    }

    public struct PointsData
    {
        #region internal
        internal IntPtr _this;

        [DllImport("MeshSyncServer")] static extern PointsData msPointsCreate();
        [DllImport("MeshSyncServer")] static extern int msPointsGetNumData(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern PointsCacheData msPointsGetData(IntPtr _this, int i);
        [DllImport("MeshSyncServer")] static extern PointsCacheData msPointsAddData(IntPtr _this);
        #endregion

        public static PointsData Create()
        {
            return msPointsCreate();
        }

        public static explicit operator PointsData(IntPtr v)
        {
            PointsData ret;
            ret._this = v;
            return ret;
        }

        public TransformData transform
        {
            get { return (TransformData)_this; }
        }

        public int numData { get { return msPointsGetNumData(_this); } }
        public PointsCacheData GetData(int i) { return msPointsGetData(_this, i); }
        public PointsCacheData AddData(int i) { return msPointsAddData(_this); }
    }
    #endregion
    #endregion


    #region Constraints
    public struct ConstraintData
    {
        #region internal
        internal IntPtr _this;
        [DllImport("MeshSyncServer")] static extern ConstraintType msConstraintGetType(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern IntPtr msConstraintGetPath(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern int msConstraintGetNumSources(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern IntPtr msConstraintGetSource(IntPtr _this, int i);
        #endregion

        public enum ConstraintType
        {
            Unknown,
            Aim,
            Parent,
            Position,
            Rotation,
            Scale,
        }

        public static explicit operator ConstraintData(IntPtr v)
        {
            ConstraintData ret;
            ret._this = v;
            return ret;
        }

        public ConstraintType type { get { return msConstraintGetType(_this); } }
        public string path { get { return Misc.S(msConstraintGetPath(_this)); } }
        public int numSources { get { return msConstraintGetNumSources(_this); } }

        public string GetSourcePath(int i) { return Misc.S(msConstraintGetSource(_this, i)); }
    }

    public struct AimConstraintData
    {
        #region internal
        internal IntPtr _this;
        #endregion


        public static explicit operator AimConstraintData(ConstraintData v)
        {
            AimConstraintData ret;
            ret._this = v._this;
            return ret;
        }
    }

    public struct ParentConstraintData
    {
        #region internal
        internal IntPtr _this;
        [DllImport("MeshSyncServer")] static extern Vector3 msParentConstraintGetPositionOffset(IntPtr _this, int i);
        [DllImport("MeshSyncServer")] static extern Quaternion msParentConstraintGetRotationOffset(IntPtr _this, int i);
        #endregion


        public static explicit operator ParentConstraintData(ConstraintData v)
        {
            ParentConstraintData ret;
            ret._this = v._this;
            return ret;
        }
        public Vector3 GetPositionOffset(int i) { return msParentConstraintGetPositionOffset(_this, i); }
        public Quaternion GetRotationOffset(int i) { return msParentConstraintGetRotationOffset(_this, i); }
    }

    public struct PositionConstraintData
    {
        #region internal
        internal IntPtr _this;
        #endregion

        public static explicit operator PositionConstraintData(ConstraintData v)
        {
            PositionConstraintData ret;
            ret._this = v._this;
            return ret;
        }
    }

    public struct RotationConstraintData
    {
        #region internal
        internal IntPtr _this;
        #endregion

        public static explicit operator RotationConstraintData(ConstraintData v)
        {
            RotationConstraintData ret;
            ret._this = v._this;
            return ret;
        }
    }

    public struct ScaleConstrainData
    {
        #region internal
        internal IntPtr _this;
        #endregion

        public static explicit operator ScaleConstrainData(ConstraintData v)
        {
            ScaleConstrainData ret;
            ret._this = v._this;
            return ret;
        }
    }
    #endregion


    #region Scene
    public struct SceneData
    {
        #region internal
        internal IntPtr _this;
        [DllImport("MeshSyncServer")] static extern IntPtr msSceneGetName(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern int msSceneGetNumObjects(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern TransformData msSceneGetObjectData(IntPtr _this, int i);
        [DllImport("MeshSyncServer")] static extern int msSceneGetNumMaterials(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern MaterialData msSceneGetMaterialData(IntPtr _this, int i);
        [DllImport("MeshSyncServer")] static extern int msSceneGetNumTextures(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern TextureData msSceneGetTextureData(IntPtr _this, int i);
        [DllImport("MeshSyncServer")] static extern int msSceneGetNumConstraints(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern ConstraintData msSceneGetConstraintData(IntPtr _this, int i);
        [DllImport("MeshSyncServer")] static extern int msSceneGetNumAnimationClips(IntPtr _this);
        [DllImport("MeshSyncServer")] static extern AnimationClipData msSceneGetAnimationClipData(IntPtr _this, int i);
        #endregion

        public string name { get { return Misc.S(msSceneGetName(_this)); } }
        public int numObjects { get { return msSceneGetNumObjects(_this); } }
        public int numMaterials { get { return msSceneGetNumMaterials(_this); } }
        public int numTextures { get { return msSceneGetNumTextures(_this); } }
        public int numConstraints { get { return msSceneGetNumConstraints(_this); } }
        public int numAnimationClips { get { return msSceneGetNumAnimationClips(_this); } }

        public TransformData GetObject(int i) { return msSceneGetObjectData(_this, i); }
        public MaterialData GetMaterial(int i) { return msSceneGetMaterialData(_this, i); }
        public TextureData GetTexture(int i) { return msSceneGetTextureData(_this, i); }
        public ConstraintData GetConstraint(int i) { return msSceneGetConstraintData(_this, i); }
        public AnimationClipData GetAnimationClip(int i) { return msSceneGetAnimationClipData(_this, i); }
    }
    #endregion Scene
}