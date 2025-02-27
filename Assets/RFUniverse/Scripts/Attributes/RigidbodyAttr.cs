﻿using UnityEngine;
using System;
using Robotflow.RFUniverse.SideChannels;

namespace RFUniverse.Attributes
{
    public class RigidbodyAttrData : ColliderAttrData
    {
        public RigidbodyData rigidbodyData;
        public RigidbodyAttrData() : base()
        {
            type = "Rigidbody";
        }
        public RigidbodyAttrData(BaseAttrData b) : base(b)
        {
            if (b is RigidbodyAttrData)
                rigidbodyData = (b as RigidbodyAttrData).rigidbodyData;
            type = "Rigidbody";
        }
    }
    [Serializable]
    public class RigidbodyData
    {
        public float mass = 1;
        public bool useGravity = true;
    }
    [RequireComponent(typeof(Rigidbody))]
    public class RigidbodyAttr : ColliderAttr
    {
        public override string Type
        {
            get { return "Rigidbody"; }
        }
        new private Rigidbody rigidbody = null;
        public Rigidbody Rigidbody
        {
            get
            {
                if (rigidbody == null)
                    rigidbody = GetComponent<Rigidbody>();
                if (rigidbody == null)
                    rigidbody = gameObject.AddComponent<Rigidbody>();
                return rigidbody;
            }
        }


        public override void Init()
        {
            base.Init();
        }

        public override BaseAttrData GetAttrData()
        {
            RigidbodyAttrData data = new RigidbodyAttrData(base.GetAttrData());
            data.rigidbodyData = GetRigidbodyData();
            return data;
        }
        public override void SetAttrData(BaseAttrData setData)
        {
            base.SetAttrData(setData);
            if (setData is RigidbodyAttrData)
            {
                RigidbodyAttrData data = setData as RigidbodyAttrData;
                SetRigidbodyData(data.rigidbodyData);
            }
        }
        private RigidbodyData rigidbodyData = new RigidbodyData();

        [EditableAttr("Rigidbody")]
        public RigidbodyData RigidbodyData
        {
            get
            {
                if (rigidbodyData == null)
                    rigidbodyData = GetRigidbodyData();
                return rigidbodyData;
            }
            set
            {
                rigidbodyData = value;
            }
        }
        public RigidbodyData GetRigidbodyData()
        {
            RigidbodyData data = new RigidbodyData();
            data.mass = Rigidbody.mass;
            data.useGravity = Rigidbody.useGravity;
            return data;
        }
        private void SetRigidbodyData(RigidbodyData data)
        {
            Rigidbody.mass = data.mass;
            Rigidbody.useGravity = data.useGravity;
        }
        private Vector3 force = Vector3.zero;
        void FixedUpdate()
        {
            if (force.magnitude > 0)
            {
                Rigidbody.AddForce(force);
                force = Vector3.zero;
            }
        }

        public override void CollectData(OutgoingMessage msg)
        {
            base.CollectData(msg);
            // Velocity
            msg.WriteFloat32(Rigidbody.velocity.x);
            msg.WriteFloat32(Rigidbody.velocity.y);
            msg.WriteFloat32(Rigidbody.velocity.z);
            // Angular velocity
            msg.WriteFloat32(Rigidbody.angularVelocity.x);
            msg.WriteFloat32(Rigidbody.angularVelocity.y);
            msg.WriteFloat32(Rigidbody.angularVelocity.z);
        }

        public override void AnalysisMsg(IncomingMessage msg, string type)
        {
            switch (type)
            {
                case "SetMass":
                    SetMass(msg);
                    return;
                case "AddForce":
                    AddForce(msg);
                    return;
                case "SetVelocity":
                    SetVelocity(msg);
                    return;
            }
            base.AnalysisMsg(msg, type);
        }

        private void SetMass(IncomingMessage msg)
        {
            float mass = msg.ReadFloat32();
            Rigidbody.mass = mass;
        }
        protected override void SetTransform(IncomingMessage msg)
        {
            base.SetTransform(msg);
            Rigidbody.velocity = Vector3.zero;
            Rigidbody.angularVelocity = Vector3.zero;
        }
        private void AddForce(IncomingMessage msg)
        {
            float x = msg.ReadFloat32();
            float y = msg.ReadFloat32();
            float z = msg.ReadFloat32();

            force = new Vector3(x, y, z);
        }

        private void SetVelocity(IncomingMessage msg)
        {
            float x = msg.ReadFloat32();
            float y = msg.ReadFloat32();
            float z = msg.ReadFloat32();

            Vector3 velocity = new Vector3(x, y, z);

            Rigidbody.velocity = velocity;
        }
    }
}

