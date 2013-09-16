using System;
using System.Collections.Generic;
using DeltaEngine.Commands;
using DeltaEngine.Content;
using DeltaEngine.Core;
using DeltaEngine.Datatypes;
using DeltaEngine.Entities;
using DeltaEngine.Extensions;
using DeltaEngine.Input;
using DeltaEngine.Multimedia;
using DeltaEngine.Rendering.Sprites;

namespace $safeprojectname$
{
	public class Ball : Sprite
	{
		public Ball(Paddle paddle) : base(new Material(Shader.Position2DColorUv, "Ball"), 
			Rectangle.Zero)
		{
			this.paddle = paddle;
			fireBallSound = ContentLoader.Load<Sound>("PaddleBallStart");
			collisionSound = ContentLoader.Load<Sound>("BallCollision");
			UpdateOnPaddle();
			RegisterFireBallCommand();
			Start<RunBall>();
			RenderLayer = 5;
		}

		private readonly Paddle paddle;
		private readonly Sound fireBallSound;
		private readonly Sound collisionSound;

		private void UpdateOnPaddle()
		{
			isOnPaddle = true;
			Position = new Point(paddle.Position.X, paddle.Position.Y - Radius);
		}

		protected bool isOnPaddle;

		private void RegisterFireBallCommand()
		{
			var command = new Command(FireBallFromPaddle);
			command.Add(new KeyTrigger(Key.Space));
			command.Add(new MouseButtonTrigger());
			command.Add(new GamePadButtonTrigger(GamePadButton.A));
		}

		private void FireBallFromPaddle()
		{
			if (!isOnPaddle || Visibility != Visibility.Show)
				return;

			isOnPaddle = false;
			float randomXSpeed = Randomizer.Current.Get(-0.15f, 0.15f);
			velocity = new Point(randomXSpeed.Abs() < 0.01f ? 0.01f : randomXSpeed, StartBallSpeedY);
			fireBallSound.Play();
		}

		protected static Point velocity;
		private const float StartBallSpeedY = -1f;

		public virtual void ResetBall()
		{
			UpdateOnPaddle();
			velocity = Point.Zero;
		}
		public class RunBall : UpdateBehavior
		{
			public override void Update(IEnumerable<Entity> entities)
			{
				foreach (var entity in entities)
				{
					var ball = (Ball)entity;
					if (ball.isOnPaddle)
						ball.UpdateOnPaddle();
					else
						ball.UpdateInFlight(Time.Delta);
					const float Aspect = 1;
					ball.DrawArea = Rectangle.FromCenter(ball.Position, new Size(Height / Aspect, Height));
				}
			}
		}
		public Point Position
		{
			get;
			protected set;
		}

		public static readonly Size BallSize = new Size(Height);
		private const float Height = Radius * 2.0f;
		internal const float Radius = 0.02f;

		protected virtual void UpdateInFlight(float timeDelta)
		{
			Position += velocity * timeDelta;
			HandleBorderCollisions();
			HandlePaddleCollision();
		}

		private void HandleBorderCollisions()
		{
			if (Position.X < Radius)
				HandleBorderCollision(Direction.Left);
			else if (Position.X > 1.0f - Radius)
				HandleBorderCollision(Direction.Right);

			if (Position.Y < Radius)
				HandleBorderCollision(Direction.Top);
			else if (Position.Y > 1.0f - Radius)
				HandleBorderCollision(Direction.Bottom);
		}
		protected enum Direction
		{
			Left,
			Top,
			Right,
			Bottom,
		}
		protected void ReflectVelocity(Direction collisionSide)
		{
			switch (collisionSide)
			{
				case Direction.Left:
					velocity.X = Math.Abs(velocity.X);
					break;
				case Direction.Top:
					velocity.Y = Math.Abs(velocity.Y);
					break;
				case Direction.Right:
					velocity.X = -Math.Abs(velocity.X);
					break;
				case Direction.Bottom:
					velocity.Y = -Math.Abs(velocity.Y);
					break;
			}
		}

		private void HandleBorderCollision(Direction collisionAtBorder)
		{
			ReflectVelocity(collisionAtBorder);
			if (collisionAtBorder == Direction.Bottom)
				ResetBall();
			else
				collisionSound.Play(0.5f);
		}

		private void HandlePaddleCollision()
		{
			if (IsInAreaOfPaddle())
				SetNewVelocityAfterPaddleCollision();
		}

		private bool IsInAreaOfPaddle()
		{
			if (Position.Y + Radius > paddle.Position.Y && velocity.Y > 0)
				return Position.X + Radius > paddle.Position.X - Paddle.HalfWidth && Position.X - Radius 
					< paddle.Position.X + Paddle.HalfWidth;

			return false;
		}

		private void SetNewVelocityAfterPaddleCollision()
		{
			velocity.X += (Position.X - paddle.Position.X) * SpeedXIncrease;
			velocity.Y = -Math.Abs(velocity.Y) * SpeedYIncrease;
			velocity.X = velocity.X.Clamp(-5f, 5f);
			velocity.Y = velocity.Y.Clamp(-5f, 0f);
			collisionSound.Play(0.6f);
		}

		private const float SpeedYIncrease = 1.015f;
		private const float SpeedXIncrease = 2.5f;

		public void Dispose()
		{
			Visibility = Visibility.Hide;
			paddle.Visibility = Visibility.Hide;
		}
	}
}