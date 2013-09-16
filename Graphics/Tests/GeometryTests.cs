﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeltaEngine.Content;
using DeltaEngine.Datatypes;
using DeltaEngine.Entities;
using DeltaEngine.Graphics.Vertices;
using DeltaEngine.Platforms;
using NUnit.Framework;

namespace DeltaEngine.Graphics.Tests
{
	public class GeometryTests : TestWithMocksOrVisually
	{
		[Test, CloseAfterFirstFrame]
		public void SetInvalidDataThrows()
		{
			var geometry =
				ContentLoader.Create<Geometry>(new GeometryCreationData(VertexFormat.Position3DColor, 1, 1));
			Assert.Throws<Geometry.InvalidNumberOfVertices>(
				() => geometry.SetData(new Vertex[] { }, new short[] { 0 }));
			Assert.Throws<Geometry.InvalidNumberOfIndices>(
				() =>
					geometry.SetData(new Vertex[] { new VertexPosition3DColor(Vector.Zero, Color.Red) },
						new short[] { }));
		}

		[Test, CloseAfterFirstFrame]
		public void LoadInvalidDataThrows()
		{
			var creationData = new GeometryCreationData(VertexFormat.Position3DColor, 1, 1);
			var geometry = ContentLoader.Create<TestGeometry>(creationData);
			Assert.Throws<Geometry.EmptyGeometryFileGiven>(geometry.LoadInvalidData);
		}

		private class TestGeometry : Geometry
		{
			public TestGeometry(GeometryCreationData creationData)
				: base(creationData) {}

			public override void Draw() {}
			protected override void SetNativeData(byte[] vertexData, short[] indices) {}
			protected override void DisposeData() {}

			public void LoadInvalidData()
			{
				LoadData(new MemoryStream());
			}
		}

		[Test]
		public void ShowTriangle()
		{
			var geometry = ContentLoader.Create<Geometry>(
				new GeometryCreationData(VertexFormat.Position3DColor, 3, 3));
			geometry.SetData(new Vertex[]
			{
				new VertexPosition3DColor(new Vector(-3.0f, 0.0f, 0.0f), Color.Red),
				new VertexPosition3DColor(new Vector(3.0f, 0.0f, 0.0f), Color.Yellow),
				new VertexPosition3DColor(new Vector(1.5f, 3.0f, 0.0f), Color.Teal)
			}, new short[] { 0, 1, 2 });
			new Triangle(geometry, new Material(Shader.Position3DColor, ""));
		}

		private class Triangle : DrawableEntity
		{
			public Triangle(Geometry geometry, Material material)
			{
				this.geometry = geometry;
				this.material = material;
				OnDraw<DrawTriangle>();
			}

			private readonly Geometry geometry;
			private readonly Material material;

			private class DrawTriangle : DrawBehavior
			{
				public DrawTriangle(Drawing drawing)
				{
					this.drawing = drawing;
				}

				private readonly Drawing drawing;

				public void Draw(IEnumerable<DrawableEntity> entities)
				{
					foreach (var triangle in entities.OfType<Triangle>())
						drawing.AddGeometry(triangle.geometry, triangle.material, Matrix.Identity);
				}
			}
		}
	}
}