using UnityEngine;
using UnityEngine.Events;
using System;
using Ashsvp;

namespace AshDev.Utility
{
    public class NitroBoost : MonoBehaviour
    {
        // Public References
        [Header("Vehicle Settings")]
        [Tooltip("Assign the vehicle's Rigidbody here.")]
        public Rigidbody vehicleRigidbody;

        public enum BoostType { Linear, Exponential, CustomCurve }
        public enum ActivationMode { SingleUse, Toggle, Hold }
        public enum BoostDirection { Forward, Backward, Custom }

        [Header("Boost Settings")]
        [Tooltip("Select the type of boost.")]
        public BoostType boostType = BoostType.Linear;

        [Tooltip("Select the mode of activation.")]
        public ActivationMode activationMode = ActivationMode.Hold;

        [Tooltip("Select the direction of the boost.")]
        public BoostDirection boostDirection = BoostDirection.Forward;

        [Tooltip("Custom boost direction if BoostDirection.Custom is selected.")]
        public Vector3 customBoostDirection = Vector3.forward;

        [Tooltip("The amount of force to apply for the boost.")]
        public float boostPower = 1000f;

        [Tooltip("The duration of the boost in seconds.")]
        public float boostDuration = 5f;

        [Tooltip("The cooldown time after the boost ends.")]
        public float boostCooldown = 10f;

        [Tooltip("Maximum speed limit for the boost. Set to 0 to ignore.")]
        public float maxSpeed = 100f;

        [Tooltip("The key used to activate the boost.")]
        public KeyCode activationKey = KeyCode.LeftShift;

        [Tooltip("Enables mobile input for activating the boost.")]
        public bool useMobileInput = false;

        [Tooltip("The UI button used to activate the boost.")]
        public UiButton_SVP activationButton;

        [Tooltip("Custom boost scaling curve (applies only if BoostType.CustomCurve is selected).")]
        public AnimationCurve boostCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Header("Visual and Audio Settings")]
        [Tooltip("Particle system or GameObject to activate during boost.")]
        public GameObject nitroVfx;

        [Tooltip("Boost start audio.")]
        public AudioClip boostStartClip;

        [Tooltip("Boost loop audio.")]
        public AudioClip boostLoopClip;

        [Tooltip("Boost end audio.")]
        public AudioClip boostEndClip;

        private AudioSource audioSource;

        [Header("Boost Events")]
        [Tooltip("All boost-related events.")]
        public BoostEvents boostEvents = new BoostEvents();

        // Events Class for Folding
        [Serializable]
        public class BoostEvents
        {
            [Tooltip("Event triggered when the boost starts.")]
            public UnityEvent OnBoostStartUnity;

            [Tooltip("Event triggered when the boost ends.")]
            public UnityEvent OnBoostEndUnity;

            [Tooltip("Event triggered when cooldown starts.")]
            public UnityEvent OnBoostCooldownStartUnity;

            [Tooltip("Event triggered when cooldown ends.")]
            public UnityEvent OnBoostCooldownEndUnity;
        }

        // Events
        public event Action OnBoostStart;
        public event Action OnBoostEnd;
        public event Action OnBoostCooldownStart;
        public event Action OnBoostCooldownEnd;

        // Private Variables
        private float boostTimer = 0f;
        private float cooldownTimer = 0f;
        private bool isBoosting = false;
        private bool isCooldownActive = false;

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.spatialBlend = 1f;

            if (nitroVfx)
            {
                nitroVfx.SetActive(false);
                OnBoostStart += () => nitroVfx.SetActive(true);
                OnBoostEnd += () => nitroVfx.SetActive(false);
            }

            OnBoostStart += () => boostEvents.OnBoostStartUnity.Invoke();
            OnBoostEnd += () => boostEvents.OnBoostEndUnity.Invoke();
            OnBoostCooldownStart += () => boostEvents.OnBoostCooldownStartUnity.Invoke();
            OnBoostCooldownEnd += () => boostEvents.OnBoostCooldownEndUnity.Invoke();
        }

        private void FixedUpdate()
        {
            HandleBoostInput();
            HandleBoostTimers();
        }

        private void HandleBoostInput()
        {
            bool isActivationPressed = Input.GetKey(activationKey) || (useMobileInput && activationButton != null && activationButton.isPressed);

            switch (activationMode)
            {
                case ActivationMode.SingleUse:
                    if (isActivationPressed && CanBoost())
                    {
                        StartBoost();
                    }
                    break;
                case ActivationMode.Toggle:
                    if (Input.GetKeyDown(activationKey) || (useMobileInput && activationButton != null && activationButton.isPressed))
                    {
                        if (!isBoosting && CanBoost())
                            StartBoost();
                        else
                            EndBoost();
                    }
                    break;
                case ActivationMode.Hold:
                    if (isActivationPressed && CanBoost())
                    {
                        StartBoost();
                    }
                    else if (!isActivationPressed)
                    {
                        EndBoost();
                    }
                    break;
            }
        }

        private void HandleBoostTimers()
        {
            if (isBoosting)
            {
                if (boostTimer > 0)
                {
                    boostTimer -= Time.fixedDeltaTime;
                    ApplyBoost();
                }
                else
                {
                    EndBoost();
                }
            }
            else if (cooldownTimer > 0)
            {
                cooldownTimer -= Time.fixedDeltaTime;

                if (!isCooldownActive)
                {
                    isCooldownActive = true;
                    OnBoostCooldownStart?.Invoke();
                }

                if (cooldownTimer <= 0)
                {
                    isCooldownActive = false;
                    OnBoostCooldownEnd?.Invoke();
                }
            }
        }

        private void StartBoost()
        {
            isBoosting = true;
            boostTimer = boostDuration;
            cooldownTimer = boostCooldown + boostDuration;
            OnBoostStart?.Invoke();
            PlayBoostStartAudio();
            PlayBoostLoopAudio();
        }

        private void PlayBoostStartAudio()
        {
            if (boostStartClip != null)
            {
                audioSource.PlayOneShot(boostStartClip);
            }
        }

        private void PlayBoostLoopAudio()
        {
            if (isBoosting && boostLoopClip != null)
            {
                audioSource.clip = boostLoopClip;
                audioSource.loop = true;
                audioSource.Play();
            }
        }

        private void ApplyBoost()
        {
            Vector3 direction = Vector3.forward;

            switch (boostDirection)
            {
                case BoostDirection.Forward:
                    direction = transform.forward;
                    break;
                case BoostDirection.Backward:
                    direction = -transform.forward;
                    break;
                case BoostDirection.Custom:
                    direction = transform.TransformDirection(customBoostDirection);
                    break;
            }

            if (vehicleRigidbody.velocity.magnitude < maxSpeed || maxSpeed <= 0)
            {
                float appliedPower = boostPower;
                if (boostType == BoostType.CustomCurve)
                {
                    float t = (boostDuration - boostTimer) / boostDuration;
                    appliedPower *= boostCurve.Evaluate(t);
                }
                else if (boostType == BoostType.Exponential)
                {
                    appliedPower *= (boostDuration - boostTimer);
                }

                vehicleRigidbody.AddForce(direction * appliedPower);
            }
        }

        private void EndBoost()
        {
            if (!isBoosting)
                return;

            isBoosting = false;
            boostTimer = 0;
            OnBoostEnd?.Invoke();
            StopBoostLoopAudio();
            PlayBoostEndAudio();
        }

        private void StopBoostLoopAudio()
        {
            audioSource.loop = false;
            audioSource.Stop();
        }

        private void PlayBoostEndAudio()
        {
            if (boostEndClip != null)
            {
                audioSource.PlayOneShot(boostEndClip);
            }
        }

        public bool CanBoost()
        {
            return cooldownTimer <= 0 && !isBoosting;
        }

        // Public Utility Methods
        public bool IsBoosting()
        {
            return isBoosting;
        }

        public float GetRemainingBoostTime()
        {
            return boostTimer;
        }

        public float GetRemainingCooldownTime()
        {
            return cooldownTimer;
        }

        public bool IsCooldownActive()
        {
            return isCooldownActive;
        }
    }
}
