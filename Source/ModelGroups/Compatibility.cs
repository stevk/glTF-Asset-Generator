﻿using glTFLoader.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;

namespace AssetGenerator
{
    internal class Compatibility : ModelGroup
    {
        public override ModelGroupName Name => ModelGroupName.Compatibility;

        public Compatibility(List<string> imageList)
        {
            NoSampleImages = true;

            // There are no common properties in this model group.

            Model CreateModel(Action<List<Property>, Runtime.GLTF, PostRuntimeChanges> setProperties)
            {
                var properties = new List<Property>();
                var meshPrimitive = MeshPrimitive.CreateSinglePlane(includeTextureCoords: false);
                var gltf = CreateGLTF(() => new Runtime.Scene()
                {
                    Nodes = new List<Runtime.Node>
                    {
                        new Runtime.Node
                        {
                            Mesh = new Runtime.Mesh
                            {
                                MeshPrimitives = new List<Runtime.MeshPrimitive>
                                {
                                    meshPrimitive
                                }
                            },
                        },
                    },
                });

                // There are no common properties in this model group.

                // Sets up a function to apply properties to the model after the Runtime layer generates the gltf.
                var postRuntimeChanges = new PostRuntimeChanges() { Function = null, };

                // Apply the properties that are specific to this gltf.
                setProperties(properties, gltf, postRuntimeChanges);

                // Create the gltf object
                return new Model
                {
                    Properties = properties,
                    GLTF = gltf,
                    PostRuntimeChanges = postRuntimeChanges.Function,
                };
            }

            void SetMinVersion(List<Property> properties, Runtime.GLTF gltf)
            {
                gltf.Asset.MinVersion = "2.1";
                properties.Add(new Property(PropertyName.MinVersion, gltf.Asset.MinVersion));
            }

            void SetVersionCurrent(List<Property> properties, Runtime.GLTF gltf)
            {
                gltf.Asset.Version = "2.0";
                properties.Add(new Property(PropertyName.Version, gltf.Asset.Version));
            }

            void SetVersionFuture(List<Property> properties, Runtime.GLTF gltf)
            {
                gltf.Asset.Version = "2.1";
                properties.Add(new Property(PropertyName.Version, gltf.Asset.Version));
            }

            void SetDescription(List<Property> properties, string description)
            {
                properties.Add(new Property(PropertyName.Description, description));
            }

            void SetDescriptionExtensionRequired(List<Property> properties, Runtime.GLTF gltf)
            {
                gltf.ExtensionsRequired = new List<string>() { "EXT_QuantumRendering" };
                gltf.Scenes[0].Nodes[0].Mesh.MeshPrimitives[0].Material = new Runtime.Material()
                {
                    Extensions = new List<Runtime.Extensions.Extension>()
                        {
                            new Runtime.Extensions.EXT_QuantumRendering()
                            {
                                PlanckFactor = new Vector4(0.2f, 0.2f, 0.2f, 0.8f),
                                CopenhagenTexture = new Runtime.Texture(),
                                EntanglementFactor = new Vector3(0.4f, 0.4f, 0.4f),
                                ProbabilisticFactor = 0.3f,
                                SuperpositionCollapseTexture = new Runtime.Texture(),
                            }
                        }
                };

                properties.Add(new Property(PropertyName.Description, "Extension required"));
            }

            void SetModelShouldLoad(List<Property> properties, string loadableStatus = ":white_check_mark:")
            {
                properties.Add(new Property(PropertyName.ModelShouldLoad, loadableStatus));
            }

            glTFLoader.Schema.Gltf SetPostRuntimeAtRoot(List<glTFLoader.Schema.Gltf> gltf)
            {
                // Add an simulated feature at the root level
                gltf[0] = new ExperimentalGltf1(gltf[0])
                {
                    lights = new ExperimentalGltf1.Light
                    {
                        Color = new float[] { 0.3f, 0.4f, 0.5f }
                    }
                };

                return gltf[0];
            }

            glTFLoader.Schema.Gltf SetPostRuntimeInProperty(List<glTFLoader.Schema.Gltf> gltf)
            {
                // Add an simulated feature into an existing property
                gltf[0].Nodes[0] = new ExperimentalGltf1.Node(gltf[0].Nodes[0])
                {
                    Light = 0.5f
                };

                return gltf[0];
            }

            glTFLoader.Schema.Gltf SetPostRuntimeWithFallback(List<glTFLoader.Schema.Gltf> gltf)
            {
                // Add an simulated feature with a fallback option
                gltf[0] = new ExperimentalGltf2(gltf[0])
                {
                    Materials = new ExperimentalGltf2.Material[]
                    {
                        new ExperimentalGltf2.Material(new glTFLoader.Schema.Material())
                        {
                            AlphaMode = glTFLoader.Schema.Material.AlphaModeEnum.BLEND,
                            AlphaMode2 = ExperimentalGltf2.Material.AlphaModeEnum.QUANTUM,
                        }
                    }
                };

                return gltf[0];
            }

            this.Models = new List<Model>
            {
                CreateModel((properties, gltf, postRuntimeChanges) => {
                    SetVersionCurrent(properties, gltf);
                    SetModelShouldLoad(properties);
                }),
                CreateModel((properties, gltf, postRuntimeChanges) => {
                    SetVersionFuture(properties, gltf);
                    SetDescription(properties, "Light object added at root");
                    SetModelShouldLoad(properties);
                    postRuntimeChanges.Function = (List<glTFLoader.Schema.Gltf> schemaGltf) => { return SetPostRuntimeAtRoot(schemaGltf); };
                }),
                CreateModel((properties, gltf, postRuntimeChanges) => {
                    SetVersionFuture(properties, gltf);
                    SetDescription(properties, "Light property added to node object");
                    SetModelShouldLoad(properties);
                    postRuntimeChanges.Function = (List<glTFLoader.Schema.Gltf> schemaGltf) => { return SetPostRuntimeInProperty(schemaGltf); };
                }),
                CreateModel((properties, gltf, postRuntimeChanges) => {
                    SetVersionFuture(properties, gltf);
                    SetDescription(properties, "Alpha mode updated with a new enum value, and a fallback value");
                    SetModelShouldLoad(properties);
                    postRuntimeChanges.Function = (List<glTFLoader.Schema.Gltf> schemaGltf) => { return SetPostRuntimeWithFallback(schemaGltf); };
                }),
                CreateModel((properties, gltf, postRuntimeChanges) => {
                    SetMinVersion(properties, gltf);
                    SetVersionFuture(properties, gltf);
                    SetDescription(properties, "Requires a specific version or higher");
                    SetModelShouldLoad(properties, "Only in version 2.1 or higher");
                }),
                CreateModel((properties, gltf, postRuntimeChanges) => {
                    SetVersionCurrent(properties, gltf);
                    SetDescriptionExtensionRequired(properties, gltf);
                    SetModelShouldLoad(properties, ":x:");
                }),
            };

            GenerateUsedPropertiesList();
        }

        private class PostRuntimeChanges
        {
            public Func<List<glTFLoader.Schema.Gltf>, glTFLoader.Schema.Gltf> Function;
        }

        // Used to add a property to the root level, or into an existing property
        private class ExperimentalGltf1 : glTFLoader.Schema.Gltf
        {
            public ExperimentalGltf1() { }
            public ExperimentalGltf1(glTFLoader.Schema.Gltf parent)
            {
                foreach (PropertyInfo property in parent.GetType().GetProperties())
                {
                    var parentProperty = property.GetValue(parent);
                    if (parentProperty != null)
                    {
                        property.SetValue(this, parentProperty);
                    }
                }
            }

            // Creates a new root level property
            public Light lights { get; set; }
            public class Light
            {
                public Light()
                {

                }

                [JsonConverter(typeof(ArrayConverter))]
                [JsonProperty("color")]
                public float[] Color { get; set; }
            }

            // Insert a feature into an existing property
            public class Node : glTFLoader.Schema.Node
            {
                public Node(glTFLoader.Schema.Node parent)
                {
                    foreach (PropertyInfo property in parent.GetType().GetProperties())
                    {
                        var parentProperty = property.GetValue(parent);
                        if (parentProperty != null)
                        {
                            property.SetValue(this, parentProperty);
                        }
                    }
                }

                [JsonConverter(typeof(ArrayConverter))]
                [JsonProperty("light")]
                public float Light { get; set; }
            }
        }

        // Used to add a new enum into an existing property with a fallback option
        private class ExperimentalGltf2 : glTFLoader.Schema.Gltf
        {
            public ExperimentalGltf2() { }
            public ExperimentalGltf2(glTFLoader.Schema.Gltf parent)
            {
                foreach (PropertyInfo property in parent.GetType().GetProperties())
                {
                    var parentProperty = property.GetValue(parent);
                    if (parentProperty != null)
                    {
                        property.SetValue(this, parentProperty);
                    }
                }
            }

            // Simulated enum
            public class Material : glTFLoader.Schema.Material
            {
                public Material(glTFLoader.Schema.Material parent)
                {
                    foreach (PropertyInfo property in parent.GetType().GetProperties())
                    {
                        var parentProperty = property.GetValue(parent);
                        if (parentProperty != null)
                        {
                            property.SetValue(this, parentProperty);
                        }
                    }
                }

                [JsonConverter(typeof(StringEnumConverter))]
                [JsonProperty("alphaMode2")]
                public AlphaModeEnum AlphaMode2 { get; set; }

                new public enum AlphaModeEnum
                {
                    OPAQUE = 0,
                    MASK = 1,
                    BLEND = 2,
                    QUANTUM = 3,
                }
            }
        }
    }
}
