using UnityEngine;
using System.Collections;

namespace NewtonVR
{
    public class NVRInteractableItem : NVRInteractable
    {
        [Tooltip("If you have a specific point you'd like the object held at, create a transform there and set it to this variable")]
        public Transform InteractionPoint;

        [Tooltip("Causes the controller model to be hidden while this item is attached.")]
        public bool HidesController = false;

        [Tooltip("Controls the rate at which the item follows the attached hand (default: 10). Higher value means the item follows the hand more responsively (perhaps at a cost of aesthetics)")]
        public float RestitutionStrength = 10f;

        protected float AttachedRotationMagic = 20f;
        protected float AttachedPositionMagic = 3000f;

        protected Transform PickupTransform;

        protected override void Awake()
        {
            base.Awake();
            this.Rigidbody.maxAngularVelocity = 100f;
        }

        protected Vector3 LastVelocityAddition;
        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (IsAttached == true)
            {
                Vector3 PositionDelta;
                Quaternion RotationDelta;

                float angle;
                Vector3 axis;

                if (InteractionPoint != null)
                {
                    RotationDelta = AttachedHand.transform.rotation * Quaternion.Inverse(InteractionPoint.rotation);
                    PositionDelta = (AttachedHand.transform.position - InteractionPoint.position);
                }
                else
                {
                    RotationDelta = PickupTransform.rotation * Quaternion.Inverse(this.transform.rotation);
                    PositionDelta = (PickupTransform.position - this.transform.position);
                }

                RotationDelta.ToAngleAxis(out angle, out axis);

                if (angle > 180)
                    angle -= 360;

                if (angle != 0)
                {
                    Vector3 AngularTarget = (Time.fixedDeltaTime * angle * axis) * AttachedRotationMagic;
                    this.Rigidbody.angularVelocity = Vector3.MoveTowards(this.Rigidbody.angularVelocity, AngularTarget, RestitutionStrength);
                }

                Vector3 VelocityTarget = PositionDelta * AttachedPositionMagic * Time.fixedDeltaTime;
                
                this.Rigidbody.velocity = Vector3.MoveTowards(this.Rigidbody.velocity, VelocityTarget, RestitutionStrength);
            }
        }

        public override void BeginInteraction(NVRHand hand)
        { 

            base.BeginInteraction(hand);

            if (HidesController)
            {
            	//turn of any renderers belonging to the hand object
                var renderers = hand.gameObject.GetComponentsInChildren<Renderer>();
                if (renderers != null) {
                    foreach (var r in renderers)
                    {
                        r.enabled = false;
                    }
                }
            }

            Vector3 closestPoint = Vector3.zero;
            float shortestDistance = float.MaxValue;
            for (int index = 0; index < Colliders.Length; index++)
            {
                Vector3 closest = Colliders[index].bounds.ClosestPoint(AttachedHand.transform.position);
                float distance = Vector3.Distance(AttachedHand.transform.position, closest);

                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    closestPoint = closest;
                }
            }

            PickupTransform = new GameObject(string.Format("[{0}] PickupTransform", this.gameObject.name)).transform;
            PickupTransform.parent = hand.transform;
            PickupTransform.position = this.transform.position;
            PickupTransform.rotation = this.transform.rotation;
        }

        public override void EndInteraction()
        {
            
            if (AttachedHand != null && HidesController)
            {
            	//reenabled any renderers for the attached hand when the item is released
                var renderers = AttachedHand.gameObject.GetComponentsInChildren<Renderer>();
                if (renderers != null) {
                    foreach (var r in renderers)
                    {
                        r.enabled = true;
                    }
                }
            }

            base.EndInteraction();

            if (PickupTransform != null)
                Destroy(PickupTransform.gameObject);
        }

    }
}