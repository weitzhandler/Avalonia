﻿using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Rendering.SceneGraph;
using Avalonia.UnitTests;
using Avalonia.VisualTree;
using Xunit;
using Avalonia.Layout;
using Avalonia.Rendering;

namespace Avalonia.Visuals.UnitTests.Rendering.SceneGraph
{
    public partial class SceneBuilderTests
    {
        [Fact]
        public void Control_With_Transparency_Should_Start_New_Layer()
        {
            using (TestApplication())
            {
                Decorator decorator;
                Border border;
                Canvas canvas;
                var tree = new TestRoot
                {
                    Padding = new Thickness(10),
                    Width = 100,
                    Height = 120,
                    Child = decorator = new Decorator
                    {
                        Padding = new Thickness(11),
                        Child = border = new Border
                        {
                            Opacity = 0.5,
                            Background = Brushes.Red,
                            Padding = new Thickness(12),
                            Child = canvas = new Canvas(),
                        }
                    }
                };

                var layout = AvaloniaLocator.Current.GetService<ILayoutManager>();
                layout.ExecuteInitialLayoutPass(tree);

                var scene = new Scene(tree);
                var sceneBuilder = new SceneBuilder();
                sceneBuilder.UpdateAll(scene);

                var rootNode = (VisualNode)scene.Root;
                var borderNode = (VisualNode)scene.FindNode(border);
                var canvasNode = (VisualNode)scene.FindNode(canvas);

                Assert.Same(tree, rootNode.LayerRoot);
                Assert.Same(border, borderNode.LayerRoot);
                Assert.Same(border, canvasNode.LayerRoot);
                Assert.Equal(0.5, scene.Layers[border].Opacity);

                Assert.Equal(2, scene.Layers.Count());
                Assert.Empty(scene.Layers.Select(x => x.LayerRoot).Except(new IVisual[] { tree, border }));

                border.Opacity = 1;
                scene = scene.Clone();

                sceneBuilder.Update(scene, border);

                rootNode = (VisualNode)scene.Root;
                borderNode = (VisualNode)scene.FindNode(border);
                canvasNode = (VisualNode)scene.FindNode(canvas);

                Assert.Same(tree, rootNode.LayerRoot);
                Assert.Same(tree, borderNode.LayerRoot);
                Assert.Same(tree, canvasNode.LayerRoot);
                Assert.Equal(1, scene.Layers.Count());

                var rootDirty = scene.Layers[tree].Dirty;

                Assert.Equal(1, rootDirty.Count());
                Assert.Equal(new Rect(21, 21, 58, 78), rootDirty.Single());
            }
        }

        [Fact]
        public void Control_With_OpacityMask_Should_Start_New_Layer()
        {
            using (TestApplication())
            {
                Decorator decorator;
                Border border;
                Canvas canvas;
                var tree = new TestRoot
                {
                    Padding = new Thickness(10),
                    Width = 100,
                    Height = 120,
                    Child = decorator = new Decorator
                    {
                        Padding = new Thickness(11),
                        Child = border = new Border
                        {
                            OpacityMask = Brushes.Red,
                            Background = Brushes.Red,
                            Padding = new Thickness(12),
                            Child = canvas = new Canvas(),
                        }
                    }
                };

                var layout = AvaloniaLocator.Current.GetService<ILayoutManager>();
                layout.ExecuteInitialLayoutPass(tree);

                var scene = new Scene(tree);
                var sceneBuilder = new SceneBuilder();
                sceneBuilder.UpdateAll(scene);

                var rootNode = (VisualNode)scene.Root;
                var borderNode = (VisualNode)scene.FindNode(border);
                var canvasNode = (VisualNode)scene.FindNode(canvas);

                Assert.Same(tree, rootNode.LayerRoot);
                Assert.Same(border, borderNode.LayerRoot);
                Assert.Same(border, canvasNode.LayerRoot);
                Assert.Equal(Brushes.Red, scene.Layers[border].OpacityMask);

                Assert.Equal(2, scene.Layers.Count());
                Assert.Empty(scene.Layers.Select(x => x.LayerRoot).Except(new IVisual[] { tree, border }));

                border.OpacityMask = null;
                scene = scene.Clone();

                sceneBuilder.Update(scene, border);

                rootNode = (VisualNode)scene.Root;
                borderNode = (VisualNode)scene.FindNode(border);
                canvasNode = (VisualNode)scene.FindNode(canvas);

                Assert.Same(tree, rootNode.LayerRoot);
                Assert.Same(tree, borderNode.LayerRoot);
                Assert.Same(tree, canvasNode.LayerRoot);
                Assert.Equal(1, scene.Layers.Count());

                var rootDirty = scene.Layers[tree].Dirty;

                Assert.Equal(1, rootDirty.Count());
                Assert.Equal(new Rect(21, 21, 58, 78), rootDirty.Single());
            }
        }

        [Fact]
        public void Removing_Transparent_Control_Should_Remove_Layers()
        {
            using (TestApplication())
            {
                Decorator decorator;
                Border border;
                Canvas canvas;
                var tree = new TestRoot
                {
                    Padding = new Thickness(10),
                    Width = 100,
                    Height = 120,
                    Child = decorator = new Decorator
                    {
                        Padding = new Thickness(11),
                        Child = border = new Border
                        {
                            Opacity = 0.5,
                            Background = Brushes.Red,
                            Padding = new Thickness(12),
                            Child = canvas = new Canvas
                            {
                                Opacity = 0.75,
                            },
                        }
                    }
                };

                var layout = AvaloniaLocator.Current.GetService<ILayoutManager>();
                layout.ExecuteInitialLayoutPass(tree);

                var scene = new Scene(tree);
                var sceneBuilder = new SceneBuilder();
                sceneBuilder.UpdateAll(scene);

                Assert.Equal(3, scene.Layers.Count);

                decorator.Child = null;
                scene = scene.Clone();

                sceneBuilder.Update(scene, border);

                Assert.Equal(1, scene.Layers.Count);
            }
        }

        [Fact]
        public void Hiding_Transparent_Control_Should_Remove_Layers()
        {
            using (TestApplication())
            {
                Decorator decorator;
                Border border;
                Canvas canvas;
                var tree = new TestRoot
                {
                    Padding = new Thickness(10),
                    Width = 100,
                    Height = 120,
                    Child = decorator = new Decorator
                    {
                        Padding = new Thickness(11),
                        Child = border = new Border
                        {
                            Opacity = 0.5,
                            Background = Brushes.Red,
                            Padding = new Thickness(12),
                            Child = canvas = new Canvas
                            {
                                Opacity = 0.75,
                            },
                        }
                    }
                };

                var layout = AvaloniaLocator.Current.GetService<ILayoutManager>();
                layout.ExecuteInitialLayoutPass(tree);

                var scene = new Scene(tree);
                var sceneBuilder = new SceneBuilder();
                sceneBuilder.UpdateAll(scene);

                Assert.Equal(3, scene.Layers.Count);

                border.IsVisible = false;
                scene = scene.Clone();

                sceneBuilder.Update(scene, border);

                Assert.Equal(1, scene.Layers.Count);
            }
        }

        [Fact]
        public void GeometryClip_Should_Affect_Child_Layers()
        {
            using (TestApplication())
            {
                var clip = StreamGeometry.Parse("M100,0 L0,100 100,100");
                Decorator decorator;
                Border border;
                var tree = new TestRoot
                {
                    Child = decorator = new Decorator
                    {
                        Clip = clip,
                        Margin = new Thickness(12, 16),
                        Child = border = new Border
                        {
                            Opacity = 0.5,
                        }
                    }
                };

                var layout = AvaloniaLocator.Current.GetService<ILayoutManager>();
                layout.ExecuteInitialLayoutPass(tree);

                var scene = new Scene(tree);
                var sceneBuilder = new SceneBuilder();
                sceneBuilder.UpdateAll(scene);

                var borderLayer = scene.Layers[border];
                Assert.Equal(
                    Matrix.CreateTranslation(12, 16),
                    ((MockStreamGeometryImpl)borderLayer.GeometryClip).Transform);
            }
        }

        [Fact]
        public void Hiding_Root_Should_Not_Remove_Root_Layer()
        {
            using (TestApplication())
            {
                Border border;
                var tree = new TestRoot
                {
                    Child = border = new Border()
                };

                var layout = AvaloniaLocator.Current.GetService<ILayoutManager>();
                layout.ExecuteInitialLayoutPass(tree);

                var scene = new Scene(tree);
                var sceneBuilder = new SceneBuilder();
                sceneBuilder.UpdateAll(scene);

                tree.IsVisible = false;

                scene = scene.Clone();
                sceneBuilder.Update(scene, tree);

                Assert.Equal(1, scene.Layers.Count);
            }
        }
    }
}
