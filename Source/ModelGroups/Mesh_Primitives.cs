﻿using System;
using System.Numerics;
using System.Collections.Generic;

namespace AssetGenerator
{
    internal class Mesh_Primitives : ModelGroup
    {
        internal override ModelGroupName Name => ModelGroupName.Mesh_Primitives;

        public Mesh_Primitives(List<string> imageList)
        {
            UseFigure(imageList, "Indices_Primitive0");
            UseFigure(imageList, "Indices_Primitive1");

            // Track the common properties for use in the readme.
            var baseColorFactorGreen = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
            var baseColorFactorBlue = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);
            CommonProperties.Add(new Property(PropertyName.Material0WithBaseColorFactor, baseColorFactorGreen));
            CommonProperties.Add(new Property(PropertyName.Material1WithBaseColorFactor, baseColorFactorBlue));

            Model CreateModel(Action<List<Property>, Runtime.MeshPrimitive, Runtime.MeshPrimitive> setProperties)
            {
                var properties = new List<Property>();
                var meshPrimitives = MeshPrimitive.CreateMultiPrimitivePlane(includeTextureCoords: false);

                // There are no common properties in this model group.

                // Apply the properties that are specific to this gltf.
                setProperties(properties, meshPrimitives[0], meshPrimitives[1]);

                // Create the gltf object
                return new Model
                {
                    Properties = properties,
                    GLTF = CreateGLTF(() => new Runtime.Scene()
                    {
                        Nodes = new List<Runtime.Node>
                        {
                            new Runtime.Node
                            {
                                Mesh = new Runtime.Mesh
                                {
                                    MeshPrimitives = meshPrimitives
                                },
                            },
                        },
                    }),
                };
            }

            void SetPrimitiveZeroGreen(List<Property> properties, Runtime.MeshPrimitive meshPrimitive)
            {
                meshPrimitive.Material = new Runtime.Material()
                {
                    MetallicRoughnessMaterial = new Runtime.PbrMetallicRoughness()
                    {
                        BaseColorFactor = baseColorFactorGreen
                    }
                };

                properties.Add(new Property(PropertyName.Primitive0, "Material 0"));
            }

            void SetPrimitiveZeroBlue(List<Property> properties, Runtime.MeshPrimitive meshPrimitive)
            {
                meshPrimitive.Material = new Runtime.Material()
                {
                    MetallicRoughnessMaterial = new Runtime.PbrMetallicRoughness()
                    {
                        BaseColorFactor = baseColorFactorBlue
                    }
                };

                properties.Add(new Property(PropertyName.Primitive0, "Material 1"));
            }

            void SetPrimitiveOneGreen(List<Property> properties, Runtime.MeshPrimitive meshPrimitive)
            {
                meshPrimitive.Material = new Runtime.Material()
                {
                    MetallicRoughnessMaterial = new Runtime.PbrMetallicRoughness()
                    {
                        BaseColorFactor = baseColorFactorGreen
                    }
                };

                properties.Add(new Property(PropertyName.Primitive1, "Material 0"));
            }

            void SetPrimitiveOneBlue(List<Property> properties, Runtime.MeshPrimitive meshPrimitive)
            {
                meshPrimitive.Material = new Runtime.Material()
                {
                    MetallicRoughnessMaterial = new Runtime.PbrMetallicRoughness()
                    {
                        BaseColorFactor = baseColorFactorBlue
                    }
                };

                properties.Add(new Property(PropertyName.Primitive1, "Material 1"));
            }

            this.Models = new List<Model>
            {
                CreateModel((properties, meshPrimitiveZero, meshPrimitiveOne) => {
                    // There are no properties set on this model.
                }),
                CreateModel((properties, meshPrimitiveZero, meshPrimitiveOne) => {
                    SetPrimitiveZeroGreen(properties, meshPrimitiveZero);
                    SetPrimitiveOneBlue(properties, meshPrimitiveOne);
                }),
                CreateModel((properties, meshPrimitiveZero, meshPrimitiveOne) => {
                    SetPrimitiveZeroBlue(properties, meshPrimitiveZero);
                    SetPrimitiveOneGreen(properties, meshPrimitiveOne);
                }),
            };

            GenerateUsedPropertiesList();
        }
    }
}
