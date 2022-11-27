using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;

public class NoiseKitEditor : EditorWindow
{
    RenderTexture canvasRT;
    NoiseGenerator ng;

    int resX = 256;
    int resY = 256;
    int resZ = 1;

    Material BlitMat;

    // Templates
    static VisualTreeAsset NoiseEditorTemplate;
    static VisualTreeAsset NoiseNodeTemplate;
    static VisualTreeAsset NoiseChannelTemplate;
    static VisualTreeAsset NoiseTemplate;
    static VisualTreeAsset PropertyTemplate;
    static VisualTreeAsset ChannelToggle;
    static VisualTreeAsset Message;

    // Noise Selection
    VisualElement SelectedNoiseList;
    Button AddNoiseButton;
    Button RemoveNoiseButton;

    // Noise Editor
    VisualElement NoiseEditorList;

    // Noise Channels
    GroupBox NoiseChannelsList;
    List<DropdownField> NoiseChannels;
    Button AddChanenlButton;
    Button RemoveChannelButton;

    // Noise Viewport
    VisualElement NoiseViewportChannels;
    List<Button> ChannelToggles;
    VisualElement NoiseViewportImage;
    Image canvasImage;


    // Noise Resolution
    Button UpdateResolutionButton;
    TextField NoiseResX;
    TextField NoiseResY;
    TextField NoiseResZ;
    List<TextField> InputResFields;

    // Noise Export
    DropdownField BitDepth;
    TextField FilePathText;
    Button SaveTextureButton;

    // Console
    ScrollView ConsoleView;
    List<VisualElement> Messages;

    // Editor
    VisualElement NoiseEditor;


    class Node 
    {
        public Node(int id)
        {
            index = id;
            editor = NoiseTemplate.Instantiate();
            editorFoldout = editor.Q<Foldout>("NoiseFoldout");
            editorProperties = editor.Q<VisualElement>("Properties");
            editorCurve = editor.Q<CurveField>("Remapper");
            selection = NoiseNodeTemplate.Instantiate();
            selectionDropdown = selection.Q<DropdownField>();
        }
        int index;

        NoiseKitUtil.Node node;
        NoiseKitUtil.NodeType type;

        public int propCount;

        VisualElement editor;
        public Foldout editorFoldout;
        public VisualElement editorProperties;
        public CurveField editorCurve;
        public AnimationCurve curve;
        public VisualElement selection;
        public DropdownField selectionDropdown;

        string outputName;

        public void InstantiateEditor(VisualElement editorParent, VisualElement selectionParent)
        {

            editorFoldout.text = "Noise (" + index.ToString() + ")";

            Keyframe key0 = new Keyframe(0, 0.0f);
            Keyframe key1 = new Keyframe(0.5f, 0.5f);
            Keyframe key2 = new Keyframe(1.0f, 1.0f);

            curve = new AnimationCurve();
            curve.AddKey(key0);
            curve.AddKey(key1);
            curve.AddKey(key2);
            editorCurve.value = curve;

            editorParent.Add(editor);

            selectionDropdown.label = "Noise (" + index.ToString() + ")";
            selectionDropdown.choices = new List<string>();

            for (int i = 0; i < Enum.GetNames(typeof(NoiseKitUtil.NodeType)).Length; i++)
            {
                NoiseKitUtil.NodeType type = (NoiseKitUtil.NodeType)i;
                selectionDropdown.choices.Add(type.ToString());
            }
            selectionDropdown.index = 0;

            selectionParent.Add(selection);

        }
        public void SetNodeType(NoiseKitUtil.NodeType _type)
        {
            type = _type;
            node = NoiseKitUtil.QueryNode(_type);
            propCount = node.properties.Count;
            outputName = "Noise_" + index.ToString();
            selectionDropdown.label = "Noise (" + index.ToString() + ")";
            editorFoldout.text = "Noise (" + index.ToString() + ")";
        }

        public NoiseKitUtil.NodeType GetNoiseType()
        {
            return type;
        }

        public NoiseKitUtil.Node GetNode()
        {
            return node;
        }

        public string GetOutputName()
        {
            return outputName;
        }

    }

    [MenuItem("Window/NoiseKit/Open _%#N")] // ctrl-shft-N
    public static void ShowWindow()
    {
        NoiseKitEditor wnd = GetWindow<NoiseKitEditor>();
        wnd.titleContent = new GUIContent("NoiseKit");
        wnd.minSize = new Vector2(280, 50);
        wnd.maxSize = new Vector2(280, 1000);
    }

    void LoadElements()
    {
        NoiseEditorTemplate = (VisualTreeAsset)AssetDatabase.LoadAssetAtPath("Packages/com.comettailz.noisekit/Editor/Resources/NoiseKitEditor.uxml", typeof(VisualTreeAsset));
        NoiseNodeTemplate = (VisualTreeAsset)AssetDatabase.LoadAssetAtPath("Packages/com.comettailz.noisekit/Editor/Resources/NoiseNode.uxml", typeof(VisualTreeAsset));
        NoiseChannelTemplate = (VisualTreeAsset)AssetDatabase.LoadAssetAtPath("Packages/com.comettailz.noisekit/Editor/Resources/NoiseChannel.uxml", typeof(VisualTreeAsset));
        NoiseTemplate = (VisualTreeAsset)AssetDatabase.LoadAssetAtPath("Packages/com.comettailz.noisekit/Editor/Resources/Noise.uxml", typeof(VisualTreeAsset));
        PropertyTemplate = (VisualTreeAsset)AssetDatabase.LoadAssetAtPath("Packages/com.comettailz.noisekit/Editor/Resources/Property.uxml", typeof(VisualTreeAsset));
        ChannelToggle = (VisualTreeAsset)AssetDatabase.LoadAssetAtPath("Packages/com.comettailz.noisekit/Editor/Resources/ChannelToggle.uxml", typeof(VisualTreeAsset));
        Message = (VisualTreeAsset)AssetDatabase.LoadAssetAtPath("Packages/com.comettailz.noisekit/Editor/Resources/Message.uxml", typeof(VisualTreeAsset));
    }
    void QueryElements()
    {
        // Noise Selection
        SelectedNoiseList = NoiseEditor.Q<VisualElement>("SelectedNoiseList");
        AddNoiseButton = NoiseEditor.Q<Button>("AddNoiseButton");
        RemoveNoiseButton = NoiseEditor.Q<Button>("RemoveNoiseButton");;

        // Noise Channels
        NoiseChannelsList = NoiseEditor.Q<GroupBox>("NoiseChannelsList");
        AddChanenlButton = NoiseEditor.Q<Button>("AddChannelButton");
        RemoveChannelButton = NoiseEditor.Q<Button>("RemoveChannelButton");

        // Noise Editor
        NoiseEditorList = NoiseEditor.Q<VisualElement>("NoiseEditorList");

        // Noise Viewport
        NoiseViewportChannels = NoiseEditor.Q<VisualElement>("ViewportChannels");
        NoiseViewportImage = NoiseEditor.Q<VisualElement>("NoiseViewportImage");

        // Noise Resolution
        InputResFields = new List<TextField>();
        UpdateResolutionButton = NoiseEditor.Q<Button>("UpdateResolutionButton");
        NoiseResX = NoiseEditor.Q<TextField>("NoiseResX");
        NoiseResY = NoiseEditor.Q<TextField>("NoiseResY");
        NoiseResZ = NoiseEditor.Q<TextField>("NoiseResZ");
        InputResFields.Clear();
        InputResFields.Add(NoiseResX);
        InputResFields.Add(NoiseResY);
        InputResFields.Add(NoiseResZ);

        // Noise Export
        BitDepth = NoiseEditor.Q<DropdownField>("BitDepthDropdown");
        SaveTextureButton = NoiseEditor.Q<Button>("SaveTextureButton");
        FilePathText = NoiseEditor.Q<TextField>("FilePathText");

        // Console
        ConsoleView = NoiseEditor.Q<ScrollView>("ConsoleView");
    }

    int nodeCount;
    List<Node> nodeList;
    List<string> outputs;
    List<string> precisions;
    List<NoiseKitUtil.Node> activeNodes;
    int channelCount;
    TextureFormat format;
    RenderTextureFormat rtFormat;
    RenderTextureReadWrite colorSpace;
    bool[] ChannelsActive;

    public void CreateGUI()
    {
        // Initialization 
        LoadElements();

        Shader blitShader = (Shader)AssetDatabase.LoadAssetAtPath("Packages/com.comettailz.noisekit/Editor/Resources/NoiseKitBlit.shader", typeof(Shader));
        BlitMat = new Material(blitShader);

        nodeCount = 0;
        nodeList = new List<Node>();
        outputs = new List<string>();
        activeNodes = new List<NoiseKitUtil.Node>();
        Node node = new Node(nodeCount);
        node.SetNodeType(NoiseKitUtil.NodeType.ValueNoise);
        nodeList.Add(node);
        nodeCount++;
        format = TextureFormat.RGBA32;
        rtFormat = RenderTextureFormat.ARGB32;
        colorSpace = RenderTextureReadWrite.sRGB;
        ChannelsActive = new bool[4];

        channelCount = 4;
        outputs = new List<string>();
        outputs.Add(node.GetOutputName());
        precisions = new List<string>();
        precisions.Add("8 Bit Per Channel");
        precisions.Add("16 Bit Per Channel");

        ng = new NoiseGenerator(resX, resY);
        canvasRT = new RenderTexture(resX, resX, resY, rtFormat, colorSpace);

        // UI Setup
        NoiseEditor = NoiseEditorTemplate.Instantiate();
        QueryElements();

        AddNoiseButton.RegisterCallback<MouseUpEvent>(AddNoiseSelection);
        RemoveNoiseButton.RegisterCallback<MouseUpEvent>(RemoveNoiseSelection);

        node.InstantiateEditor(NoiseEditorList, SelectedNoiseList);
        node.editorCurve.RegisterValueChangedCallback(UpdateCurveValue);
        node.selectionDropdown.RegisterValueChangedCallback(UpdateNodeTypes);

        UpdateNoiseEditors();

        UpdateOutputs();

        NoiseChannels = new List<DropdownField>();
        VisualElement channelR = NoiseChannelTemplate.Instantiate();
        DropdownField fieldR = channelR.Q<DropdownField>("ChannelField");
        fieldR.label = "Red Channel";
        fieldR.index = 0;
        NoiseChannels.Add(fieldR);
        NoiseChannelsList.Add(channelR);
        VisualElement channelG = NoiseChannelTemplate.Instantiate();
        DropdownField fieldG = channelG.Q<DropdownField>("ChannelField");
        fieldG.label = "Green Channel";
        NoiseChannels.Add(fieldG);
        NoiseChannelsList.Add(channelG);
        VisualElement channelB = NoiseChannelTemplate.Instantiate();
        DropdownField fieldB = channelB.Q<DropdownField>("ChannelField");
        fieldB.label = "BlueChannel";
        NoiseChannels.Add(fieldB);
        NoiseChannelsList.Add(channelB);
        VisualElement channelA = NoiseChannelTemplate.Instantiate();
        DropdownField fieldA = channelA.Q<DropdownField>("ChannelField");
        fieldA.label = "Alpha Channel";
        NoiseChannels.Add(fieldA);
        NoiseChannelsList.Add(channelA);

        NoiseChannels[0].RegisterValueChangedCallback(UpdateOutputsSelection);
        NoiseChannels[1].RegisterValueChangedCallback(UpdateOutputsSelection);
        NoiseChannels[2].RegisterValueChangedCallback(UpdateOutputsSelection);
        NoiseChannels[3].RegisterValueChangedCallback(UpdateOutputsSelection);
        AddChanenlButton.RegisterCallback<MouseUpEvent>(AddNoiseChannel);
        RemoveChannelButton.RegisterCallback<MouseUpEvent>(RemoveNoiseChannel);

        UpdateChannelFields();

        // Resolution Settings
        NoiseResX.value = resX.ToString();
        NoiseResY.value = resY.ToString();
        //NoiseResZ.value = resZ.ToString();
        UpdateResolutionButton.RegisterCallback<PointerUpEvent, List<TextField>>(UpdateRes, InputResFields);

        // File Settings
        BitDepth.choices = precisions;
        BitDepth.index = 0;
        BitDepth.RegisterValueChangedCallback(UpdateFormat);
        SaveTextureButton.RegisterCallback<PointerUpEvent, TextField>(SaveTex, FilePathText);

        // Cavnas
        ChannelToggles = new List<Button>();
        VisualElement toggleR = ChannelToggle.Instantiate();
        Button buttonR = toggleR.Q<Button>("Button");
        buttonR.text = "R";
        ChannelsActive[0] = true;
        ChannelToggles.Add(buttonR);
        buttonR.RegisterCallback<PointerUpEvent, Button>(ToggleChannels, buttonR);
        VisualElement toggleG = ChannelToggle.Instantiate();
        Button buttonG = toggleG.Q<Button>("Button");
        buttonG.text = "G";
        ChannelsActive[1] = true;
        ChannelToggles.Add(buttonG);
        buttonG.RegisterCallback<PointerUpEvent, Button>(ToggleChannels, buttonG);
        VisualElement toggleB = ChannelToggle.Instantiate();
        Button buttonB = toggleB.Q<Button>("Button");
        buttonB.text = "B";
        ChannelsActive[2] = true;
        ChannelToggles.Add(buttonB);
        buttonB.RegisterCallback<PointerUpEvent, Button>(ToggleChannels, buttonB);
        VisualElement toggleA = ChannelToggle.Instantiate();
        Button buttonA = toggleA.Q<Button>("Button");
        buttonA.text = "A";
        buttonA.style.opacity = 0.25f;
        ChannelsActive[3] = false;
        ChannelToggles.Add(buttonA);
        buttonA.RegisterCallback<PointerUpEvent, Button>(ToggleChannels, buttonA);
        NoiseViewportChannels.Add(toggleA);
        NoiseViewportChannels.Add(toggleB);
        NoiseViewportChannels.Add(toggleG);
        NoiseViewportChannels.Add(toggleR);

        SetupCanvas(NoiseViewportImage);

        Messages = new List<VisualElement>();
        VisualElement message = Message.Instantiate();
        Messages.Add(message);
        ConsoleView.Add(message);
        Label log = Messages[0].Q<Label>("MessageLabel");
        log.text = "NoiseKit: Version 0.0.1";

        var root = rootVisualElement;
        root.Add(NoiseEditor);

        // Compute Setup and Dispatch
        ng.SetupNodes(GetActiveNodes());
        GenerateShader();
        ng.SetupCompute();
        ng.DispatchCompute();
        UpdateCanvas();
        


    }
    private List<NoiseKitUtil.Node> GetActiveNodes()
    {
        activeNodes.Clear();
        for (int i = 0; i < nodeList.Count; i++)
        {
            activeNodes.Add(nodeList[i].GetNode());
        }
        return activeNodes;
    }
    private void AddNoiseSelection(MouseUpEvent evt)
    {
        Node node = new Node(nodeCount);
        node.SetNodeType(NoiseKitUtil.NodeType.ValueNoise);
        nodeList.Add(node);
        nodeCount++;

        node.InstantiateEditor(NoiseEditorList, SelectedNoiseList);
        node.editorCurve.RegisterValueChangedCallback(UpdateCurveValue);
        node.selectionDropdown.RegisterValueChangedCallback(UpdateNodeTypes);

        UpdateNoiseEditors();
        UpdateOutputs();
        UpdateChannelFields();

        ng.SetupNodes(GetActiveNodes());
        GenerateShader();
        ng.SetupCompute();
        ng.DispatchCompute();

        UpdateCanvas();
    }
    private void RemoveNoiseSelection(MouseUpEvent evt)
    {
        nodeList.RemoveAt(nodeCount -1);
        nodeCount--;

        NoiseEditorList.Clear();
        SelectedNoiseList.Clear();
        for (int i=0; i<nodeList.Count; i++)
        {
            nodeList[i].InstantiateEditor(NoiseEditorList, SelectedNoiseList);
        }

        UpdateNoiseEditors();
        UpdateOutputs();
        UpdateChannelFields();

        ng.SetupNodes(GetActiveNodes());
        GenerateShader();
        ng.SetupCompute();
        ng.DispatchCompute();

        UpdateCanvas();

    }
    private void AddNoiseChannel(MouseUpEvent evt)
    {
        if (channelCount > 0 && channelCount < 4)
        {
            channelCount++;
            VisualElement channel = NoiseChannelTemplate.Instantiate();
            DropdownField channelField = channel.Q<DropdownField>("ChannelField");
            channelField.RegisterValueChangedCallback(UpdateOutputsSelection);
            NoiseChannelsList.Add(channel);

            if (channelCount == 1)
            {
                channelField.label = "Red Channel";
            }
            else if (channelCount == 2)
            {
                channelField.label = "Green Channel";
            }
            else if (channelCount == 3)
            {
                channelField.label = "Blue Channel";
            }
            else if (channelCount == 4)
            {
                channelField.label = "Alpha Channel";
            }

            channelField.choices = outputs;
            channelField.index = 0;
            NoiseChannels.Add(channelField);
            NoiseChannelsList.Add(channel);

            UpdateTextureFormat();
            canvasRT = new RenderTexture(resX, resY, resZ, rtFormat, colorSpace);
            ng.UpdateRT(resX, resY, resZ, rtFormat, colorSpace);
            ng.DispatchCompute();
            UpdateCanvas();
        }
    }
    private void RemoveNoiseChannel(MouseUpEvent evt)
    {
        if (channelCount > 1 && channelCount <= 4)
        {
            NoiseChannels.RemoveAt(channelCount - 1);
            NoiseChannelsList.RemoveAt(channelCount-1);
            channelCount--;

            UpdateTextureFormat();
            canvasRT = new RenderTexture(resX, resY, resZ, rtFormat, colorSpace);
            ng.UpdateRT(resX, resY, resZ, rtFormat, colorSpace);
            ng.DispatchCompute();
            UpdateCanvas();
        }
    }
    private void SetupCanvas(VisualElement canvas)
    {
        canvasImage = new Image();
        canvasImage.image = canvasRT;
        canvasImage.scaleMode = ScaleMode.StretchToFill;

        canvas.Add(canvasImage);
        canvas.tooltip = canvas.parent.name;
    }
    private void UpdateCanvas()
    {
        if (canvasRT == null)
        {
            canvasRT = new RenderTexture(resX, resY, resZ, rtFormat, colorSpace);
        }

        BlitMat.SetFloat("_Red", ChannelsActive[0] ? 1.0f : 0.0f);
        BlitMat.SetFloat("_Green", ChannelsActive[1] ? 1.0f : 0.0f);
        BlitMat.SetFloat("_Blue", ChannelsActive[2] ? 1.0f : 0.0f);
        BlitMat.SetFloat("_Alpha", ChannelsActive[3] ? 1.0f : 0.0f);
        RenderTexture previousActiveRT = RenderTexture.active;
        Graphics.Blit(ng.GetTex(), canvasRT, BlitMat);
        RenderTexture.active = previousActiveRT;
        canvasImage.image = canvasRT;
    }
    private void UpdateNodeTypes(ChangeEvent<string> evt)
    {
        for (int i= 0; i < nodeList.Count; i++)
        {
            nodeList[i].SetNodeType(NoiseKitUtil.NodeTypeFromString(nodeList[i].selectionDropdown.value)); 
        }
        
        UpdateNoiseEditors();

        ng.SetupNodes(GetActiveNodes());
        GenerateShader();
        ng.SetupCompute();
        ng.DispatchCompute();

        UpdateCanvas();

    }
    private void UpdateNoiseEditors()
    {
        for (int i = 0; i < nodeList.Count; i++)
        {
            nodeList[i].editorProperties.Clear();
            for (int p = 0; p < nodeList[i].GetNode().properties.Count; p++)
            {
                VisualElement property = PropertyTemplate.Instantiate();
                Label propName = property.Q<Label>("PropertyName");
                propName.text = nodeList[i].GetNode().properties[p].name;
                Slider propSlider = property.Q<Slider>("PropertySlider");
                propSlider.lowValue = nodeList[i].GetNode().properties[p].min;
                propSlider.highValue = nodeList[i].GetNode().properties[p].max;
                propSlider.value = nodeList[i].GetNode().properties[p].val;
                propSlider.RegisterValueChangedCallback(UpdateSliderValues);
                nodeList[i].editorProperties.Add(property);
            }   
        }
    }
    private void UpdateOutputs()
    {
        outputs.Clear();

        for (int i = 0; i < nodeList.Count; i++)
        {
            outputs.Add(nodeList[i].GetOutputName());
        }
    }
    private void UpdateChannelFields()
    {
        for (int i = 0; i < channelCount; i++)
        {
            NoiseChannels[i].choices = outputs;
            NoiseChannels[i].index = 0;
        }
    }
    private void UpdateOutputsSelection(ChangeEvent<string> evt)
    {
        GenerateShader();
        ng.SetupCompute();
        UpdateProperties();
        ng.DispatchCompute();
        UpdateCanvas();
    }
    private void UpdateSliderValues(ChangeEvent<float> evt)
    {
        UpdateProperties();
        ng.DispatchCompute();
        UpdateCanvas();
    }
    private void UpdateProperties()
    {
        for (int i = 0; i < nodeList.Count; i++)
        {
            for (int p = 0; p < nodeList[i].GetNode().properties.Count; p++)
            {
                ng.propsList[i][p].value = nodeList[i].editorProperties[p].Q<Slider>("PropertySlider").value;
            }

        }
    }
    private void UpdateCurveValue(ChangeEvent<AnimationCurve> curve)
    {
        for (int i = 0; i < nodeCount; i++)
        {
            nodeList[i].curve = curve.newValue;
            Vector3 cps = new Vector3(nodeList[i].curve.keys[0].value, nodeList[i].curve.keys[1].value, nodeList[i].curve.keys[2].value);
            ng.curveList[i] = cps;
            ng.UpdateCurveBuffer();
            ng.DispatchCompute();
            UpdateCanvas();
        }
    }
    private void UpdateRes(PointerUpEvent evnt, List<TextField> inputRes)
    {
        resX = Int32.Parse(inputRes[0].value);
        if (resX > 8)
        {
            if (resX % 8 != 0) { resX = (int)(Mathf.Floor((float)resX / 8.0f) * 8.0f); inputRes[0].value = resX.ToString(); } 
        } else { resX = 8; inputRes[0].value = resX.ToString(); }

        resY = Int32.Parse(inputRes[1].value);
        if (resY > 8)
        {
            if (resY % 8 != 0) { resY = (int)(Mathf.Floor((float)resY / 8.0f) * 8.0f); inputRes[1].value = resY.ToString(); }
        }
        else { resY = 8; inputRes[1].value = resY.ToString(); }
        resZ = 1;
        canvasRT = new RenderTexture(resX, resY, resZ, rtFormat, colorSpace);
        ng.UpdateRT(resX, resY, resZ, rtFormat, colorSpace);
        ng.DispatchCompute();
        UpdateCanvas();
    }
    private void GenerateShader()
    {
        string src = "";
        string srcPragma = "";
        string srcTarget = "";
        string srcProps = "";
        string srcCurves = "";
        string srcRes = ""; 
        string srcKernel = "";
        string srcIncludes = "";

        srcIncludes = "#include \"Packages/com.comettailz.noisekit/Editor/Resources/NoiseKit.hlsl\" \n";
        srcPragma = "#pragma kernel NoiseKernel \n";
        if (channelCount == 1) { srcTarget = "RWTexture2D<float> _outputTex; \n".Replace("\n", Environment.NewLine); }
        else if (channelCount == 2) { srcTarget = "RWTexture2D<float2> _outputTex; \n".Replace("\n", Environment.NewLine); }
        else if (channelCount == 3) { srcTarget = "RWTexture2D<float3> _outputTex; \n".Replace("\n", Environment.NewLine); }
        else if (channelCount == 4) { srcTarget = "RWTexture2D<float4> _outputTex; \n".Replace("\n", Environment.NewLine); }
        
        for(int i=0; i<nodeList.Count; i++)
        {
            srcProps += "StructuredBuffer<float> _propsBuffer" + i.ToString() + ";\n".Replace("\n", Environment.NewLine);
            srcCurves += "StructuredBuffer<float> _curveBuffer" + i.ToString() + ";\n".Replace("\n", Environment.NewLine);
        }
        srcRes = "float4 _res; \n".Replace("\n", Environment.NewLine);

        srcKernel = "[numthreads(8,8,1)] \nvoid NoiseKernel (uint3 id : SV_DispatchThreadID) \n{ \nfloat2 uv = id.xy / _res.xy; \n".Replace("\n", Environment.NewLine);
        for (int i = 0; i < ng.nodeList.Count; i++)
        {
            for (int k = 0; k < ng.nodeList[i].source.Count; k++)
            {
                srcKernel += ng.nodeList[i].source[k].Replace("$", nodeList[i].GetOutputName()).Replace("#", i.ToString()).Replace("\n", Environment.NewLine);
            }
        }
        srcKernel += "float r = " + NoiseChannels[0].value + ";\n".Replace("\n", Environment.NewLine);
        string channels = "r";
        if (channelCount > 1) { srcKernel += "float g = " + NoiseChannels[1].value + ";\n".Replace("\n", Environment.NewLine); channels = "float2(r, g)"; }
        if (channelCount > 2) { srcKernel += "float b = " + NoiseChannels[2].value + ";\n".Replace("\n", Environment.NewLine); channels = "float3(r, g, b)"; }
        if (channelCount > 3) { srcKernel += "float a = " + NoiseChannels[3].value + ";\n".Replace("\n", Environment.NewLine); channels = "float4(r, g, b, a)"; }
        srcKernel += "_outputTex[id.xy] = " + channels + ";} \n".Replace("\n", Environment.NewLine);

        src = srcPragma + System.Environment.NewLine + srcIncludes + System.Environment.NewLine + srcTarget + System.Environment.NewLine + srcProps + System.Environment.NewLine + srcCurves + System.Environment.NewLine + System.Environment.NewLine + srcRes + System.Environment.NewLine + srcKernel;
        System.IO.File.WriteAllText("Packages/com.comettailz.noisekit/Editor/Resources/ComputeNoise.compute", src);

        AssetDatabase.Refresh();
        ng.computeShader = (ComputeShader)AssetDatabase.LoadAssetAtPath("Packages/com.comettailz.noisekit/Editor/Resources/ComputeNoise.compute", typeof(ComputeShader));

    }
    private void UpdateFormat(ChangeEvent<string> evt)
    {
        UpdateTextureFormat();
        canvasRT = new RenderTexture(resX, resY, resZ, rtFormat, colorSpace);
        ng.UpdateRT(resX, resY, resZ, rtFormat, colorSpace);
        ng.DispatchCompute();
        UpdateCanvas();
    }
    private void UpdateTextureFormat()
    {
        if (channelCount == 1)
        {
            if (BitDepth.index == 0) // 8 bit per channel
            {
                format = TextureFormat.R8;
                rtFormat = RenderTextureFormat.R8;
                colorSpace = RenderTextureReadWrite.sRGB;
            }
            else if (BitDepth.index == 1) // 16 bit per channel
            {
                format = TextureFormat.RHalf;
                rtFormat = RenderTextureFormat.RHalf;
                colorSpace = RenderTextureReadWrite.Linear;
            }
        }
        else if (channelCount == 2)
        {
            if (BitDepth.index == 0)
            {
                format = TextureFormat.RG16;
                rtFormat = RenderTextureFormat.RG16;
                colorSpace = RenderTextureReadWrite.sRGB;
            }
            else if (BitDepth.index == 1)
            {
                format = TextureFormat.RGHalf;
                rtFormat = RenderTextureFormat.RGHalf;
                colorSpace = RenderTextureReadWrite.Linear;
            }
        }
        else if (channelCount >= 3)
        {
            if (BitDepth.index == 0)
            {
                format = TextureFormat.RGBA32;
                rtFormat = RenderTextureFormat.ARGB32;
                colorSpace = RenderTextureReadWrite.sRGB;
            }
            else if (BitDepth.index == 1)
            {
                format = TextureFormat.RGBAHalf;
                rtFormat = RenderTextureFormat.ARGBHalf;
                colorSpace = RenderTextureReadWrite.Linear;
            }
        }
    }
    private void ToggleChannels(PointerUpEvent evnt, Button button)
    {
        if ( button.text == "R")
        {
            ChannelsActive[0] = !ChannelsActive[0];
            if (ChannelsActive[0]) { button.style.opacity = 1; } else { button.style.opacity = 0.25f; }
        } else if (button.text == "G")
        {
            ChannelsActive[1] = !ChannelsActive[1];
            if (ChannelsActive[1]) { button.style.opacity = 1; } else { button.style.opacity = 0.25f; }
        } else if (button.text == "B")
        {
            ChannelsActive[2] = !ChannelsActive[2];
            if (ChannelsActive[2]) { button.style.opacity = 1; } else { button.style.opacity = 0.25f; }
        } else if (button.text == "A")
        {
            ChannelsActive[3] = !ChannelsActive[3];
            if (ChannelsActive[3]) { 
                button.style.opacity = 1;
                ChannelsActive[0] = false;
                ChannelToggles[0].style.opacity = 0.25f;
                ChannelsActive[1] = false;
                ChannelToggles[1].style.opacity = 0.25f;
                ChannelsActive[2] = false;
                ChannelToggles[2].style.opacity = 0.25f;
            } else { button.style.opacity = 0.25f; }
        }

        canvasRT = new RenderTexture(resX, resY, resZ, rtFormat, colorSpace);
        UpdateCanvas();
    }

    Texture2D texRead;
    Texture2D texTmp;
    private void SaveTex(PointerUpEvent cursorReleaseEvent, TextField input)
    {
        RenderTexture rt = ng.GetTex();
        bool usingLinearSpace;
        if ( colorSpace  == RenderTextureReadWrite.sRGB ) { usingLinearSpace = false; } else { usingLinearSpace = true; texTmp = new Texture2D(rt.width, rt.height, format, usingLinearSpace); }
        texRead = new Texture2D(rt.width, rt.height, format, usingLinearSpace);
        RenderTexture previousActiveRT = RenderTexture.active;
        RenderTexture.active = rt;
        texRead.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0); // copy data from active RT
        if (BitDepth.index == 1)
        {
            for (int x = 0; x < resX; x++)
            {
                for (int y = 0; y < resY; y++)
                {
                    Color col;
                    col = texRead.GetPixel(x, y);
                    texTmp.SetPixel(x, y, col.linear); // manual conversion from sRGB to linear is ineeded, even though RTs and tex are using linear space...   
                }
            }
        }
        byte[] texData;
        string extension;
        if (BitDepth.index == 0)
        {
            texData = texRead.EncodeToPNG();
            extension = ".png";
        } else 
        {
            texData = texTmp.EncodeToEXR();
            extension = ".exr";
        }
        RenderTexture.active = previousActiveRT;
        string path = Application.dataPath + "/" + input.text + extension;

        System.IO.FileInfo fileInfo = new System.IO.FileInfo(path);
        fileInfo.Directory.Create();
        System.IO.File.WriteAllBytes(fileInfo.FullName, texData);
        AssetDatabase.Refresh();

        Label log = Messages[0].Q<Label>("MessageLabel");
        log.text = "NoiseKit: saved noise texture to " + path;
    }

}
