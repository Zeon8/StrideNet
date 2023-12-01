using Riptide;
using Stride.Core.Annotations;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Input;
using StrideNet;
using System;

namespace Sample
{
    public partial class Player : NetworkScript
    {
        public float Speed { get; init; }

        [DataMemberRange(0,1, 0.01, 0.1, 5)]
        public float Interpolation { get; init; } = 0.5f;

        [NetworkVariable(SendMode = MessageSendMode.Unreliable)]
        private Vector3 _position;

		public override void Start()
        {
            base.Start();
            Log.Info($"NetworkId: {OwnerId}, IsLocalPlayer: {IsOwner}");
        }

        public override void Update()
        {
            if(IsClient)
                Entity.Transform.Position = Vector3.Lerp(Entity.Transform.Position, Position, Interpolation);

            if (!IsOwner)
                return;

            float horizontal = (Input.IsKeyDown(Keys.A) ? -1f : 0) + (Input.IsKeyDown(Keys.D) ? 1f : 0);
            float vertical = (Input.IsKeyDown(Keys.S) ? -1f : 0) + (Input.IsKeyDown(Keys.W) ? 1f : 0);

            Vector2 input = new(horizontal, vertical);
            UpdatePositionRpc(input);
        }

        [NetworkRpc]
        private void UpdatePosition(Vector2 input)
        {
            DebugText.Print($"Input: {input}", new Int2(15, 15));
            float deltaTime = (float)Game.UpdateTime.Elapsed.TotalSeconds;
            Entity.Transform.Position += Entity.Transform.WorldMatrix.Up * Speed * input.Y * deltaTime;
            Entity.Transform.Position += Entity.Transform.WorldMatrix.Right * Speed * input.X * deltaTime;

            Position = Entity.Transform.Position;
        }
    }
}
