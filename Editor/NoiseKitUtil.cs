using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NoiseKitUtil
{

    public static int counter;

    public enum NodeType
    {
        ValueNoise = 0,
        PerlinNoise = 1,
        CellularNoise = 2,
        FractalValueNoise = 3,
        FractalPerlinNoise = 4
    }

    public static Node QueryNode(NodeType type)
    {
        if (type == NodeType.ValueNoise)
        {
            return ValueNoise;
        }
        else if (type == NodeType.PerlinNoise)
        {
            return PerlinNoise;
        }
        else if (type == NodeType.CellularNoise)
        {
            return CellularNoise;
        }
        else if (type == NodeType.FractalValueNoise)
        {
            return FractalValueNoise;
        }
        else if (type == NodeType.FractalPerlinNoise)
        {
            return FractalPerlinNoise;
        }
        else return ValueNoise;
    }

    public static NodeType NodeTypeFromString(string name)
    {
        if (name == "ValueNoise")
        {
            return NodeType.ValueNoise;
        }
        else if (name == "PerlinNoise")
        {
            return NodeType.PerlinNoise;
        }
        else if (name == "CellularNoise")
        {
            return NodeType.CellularNoise;
        }
        else if (name == "FractalValueNoise")
        {
            return NodeType.FractalValueNoise;
        }
        else if (name == "FractalPerlinNoise")
        {
            return NodeType.FractalPerlinNoise;
        }
        else return NodeType.ValueNoise;
    }

    public class Node
    {
        public List<Property> properties;
        public List<string> functions;
        public List<string> source;

        public Node()
        {
        }

        public Node (List<Property> _properties, List<string> _functions, List<string> _source)
        {
            properties = _properties;
            functions = _functions;
            source = _source;
        }

        List<string> GetFunctions()
        {
            return functions;
        }
        List<string> GetSource()
        {
            return source;
        }

    }

    public struct Property
    {
        public float val;
        public float min;
        public float max;
        public string name;
        public Property(string _name, float _val, float _min, float _max)
        {
            val = _val;
            min = _min; 
            max = _max; 
            name = _name;
        }
      
    }

    public static Node ValueNoise = new Node(
            new List<Property>()
            {
                new Property ("Seed", Random.Range(0.0f, 50.0f), 0.0f, 50.0f)
            },
            new List<string>(){
            },
            new List<string>(){
                //float valueNoise2D(float2 uv, float2 dir, float2 period)
                "float $ = valueNoise2D(uv * 4, float2(_propsBuffer#[0], _propsBuffer#[0] / 2.71828), float2(4, 4), _curveBuffer#[0], _curveBuffer#[1], _curveBuffer#[2]);\n"
            }
    );

    public static Node FractalValueNoise = new Node(
        new List<Property>()
        {
                    new Property ("Seed", Random.Range(0.0f, 0.5f), 0.0f, 0.5f),
                    new Property ("Amplitude", 1.0f, 0.0f, 1.5f),
                    new Property ("Frequency", 0.0f, 0.0f, 5.0f),
                    new Property ("Octaves", 2.0f, 0.0f, 4.0f),
                    new Property ("Gain", 0.75f, 0.0f, 1.0f),
                    new Property ("Lacunarity", 0.0f, 0.0f, 30)
        },
        new List<string>()
        {
        },
        new List<string>(){
                    "float $ = fractalValueNoise2D(uv * 4, float2(_propsBuffer#[0], _propsBuffer#[0] / 2.71828), _propsBuffer#[1], _propsBuffer#[2], _propsBuffer#[3], _propsBuffer#[4], _propsBuffer#[5], float2(4,4), 0.4, _curveBuffer#[0], _curveBuffer#[1], _curveBuffer#[2]);\n"
        }
    );

    public static Node PerlinNoise = new Node(
        new List<Property>()
        {
                new Property ("Seed", Random.Range(0.0f, 50.0f), 0.0f, 50.0f)
        },
        new List<string>()
        {
        },
        new List<string>(){
                //float perlinNoise2D(float2 uv, float2 dir, float2 period)
                "float $ = perlinNoise2D(uv * 4, float2(_propsBuffer#[0], _propsBuffer#[0] / 2.718), float2(4, 4), _curveBuffer#[0], _curveBuffer#[1], _curveBuffer#[2]);\n"
        }
    );

    public static Node FractalPerlinNoise = new Node(
         new List<Property>()
         {
                new Property ("Seed", Random.Range(0.0f, 0.5f), 0.0f, 0.5f),
                new Property ("Amplitude", 1.0f, 0.0f, 1.5f),
                new Property ("Frequency", 0.0f, 0.0f, 5.0f),
                new Property ("Octaves", 2.0f, 0.0f, 4.0f),
                new Property ("Gain", 0.75f, 0.0f, 1.0f),
                new Property ("Lacunarity", 0.0f, 0.0f, 25.0f)
         },
         new List<string>()
         {
         },
         new List<string>(){

                "float $ = fractalPerlinNoise2D(uv * 4, float2(_propsBuffer#[0], _propsBuffer#[0] / 2.71828), _propsBuffer#[1], _propsBuffer#[2], _propsBuffer#[3], _propsBuffer#[4], _propsBuffer#[5], float2(4,4), 0.4, _curveBuffer#[0], _curveBuffer#[1], _curveBuffer#[2]);\n"
         }
     );

        public static Node CellularNoise = new Node(
        new List<Property>()
        {
                        new Property ("Seed", Random.Range(0.0f, 50.0f), 0.0f, 50.0f)
        },
        new List<string>()
        {
        },
        new List<string>(){
                        "float $ = cellularNoise2D(uv , float2(_propsBuffer#[0], _propsBuffer#[0]), 4.0, _curveBuffer#[0], _curveBuffer#[1], _curveBuffer#[2]);\n"
        }
    );

    public static float SampleBezier(float t, float[] cp)
    {
        float val = (1.0f - t) * (1.0f - t) * cp[0] + 2.0f * (1.0f - t) * t * cp[1] + t * t * cp[2];
        return val;
    }
}
