using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using NeonDrift.Vehicle;
using NeonDrift.World;
using NeonDrift.Core;
using NeonDrift.Gameplay;

namespace NeonDrift.Diagnostics
{
    public class VehicleStopDiagnostic : MonoBehaviour
    {
        [System.Serializable]
        public struct FrameRecord
        {
            public int frame;
            public float time;
            public Vector2 pos;
            public float speed;
            public float speedDelta;
            public Vector2 linVel;
            public float angVel;
            public string state;
            public float steer;
            public bool drift;
            public bool boost;
            public bool autoAcc;
            public string gmState;
            public int chunkCount;
            public Vector2 camPos;
            public string lastContact;
            public Vector2 contactNormal;
        }

        private VehicleController vehicle;
        private Rigidbody2D rb;
        private GameManager gm;
        private TrackManager tm;
        private Camera mainCam;

        private const int BUFFER_SIZE = 60;
        private readonly FrameRecord[] ringBuffer = new FrameRecord[BUFFER_SIZE];
        private int bufferIndex = 0;
        private int totalFrames = 0;

        private float prevSpeed = 0f;
        private string latestContactInfo = "None";
        private Vector2 latestContactNormal = Vector2.zero;
        private float contactTimer = 0f;

        private bool eventTriggered = false;
        private int postEventFramesRemaining = 0;
        private StringBuilder eventReportBuilder = null;

        private void Awake()
        {
            vehicle = GetComponent<VehicleController>();
            rb = GetComponent<Rigidbody2D>();
            gm = UnityEngine.Object.FindFirstObjectByType<GameManager>();
            tm = UnityEngine.Object.FindFirstObjectByType<TrackManager>();
            mainCam = Camera.main;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            RecordCollision("Enter", collision);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            RecordCollision("Stay", collision);
        }

        private void RecordCollision(string type, Collision2D collision)
        {
            if (collision.contactCount > 0)
            {
                var contact = collision.GetContact(0);
                latestContactInfo = $"[{type}] with '{collision.gameObject.name}' (Tag: {collision.gameObject.tag}, Layer: {LayerMask.LayerToName(collision.gameObject.layer)}) at pt {contact.point}";
                latestContactNormal = contact.normal;
                contactTimer = 0.5f;

                Debug.LogWarning($"[DIAGNOSTIC COLLISION] {latestContactInfo} | Normal: {latestContactNormal} | RelVel: {collision.relativeVelocity.magnitude:F2}");
            }
        }

        private void FixedUpdate()
        {
            if (vehicle == null || rb == null) return;

            totalFrames++;
            if (contactTimer > 0f)
            {
                contactTimer -= Time.fixedDeltaTime;
                if (contactTimer <= 0f)
                {
                    latestContactInfo = "None";
                    latestContactNormal = Vector2.zero;
                }
            }

            float currentSpeed = vehicle.currentSpeed;
            float speedDelta = currentSpeed - prevSpeed;
            Vector2 linVel = rb.linearVelocity;

            FrameRecord record = new FrameRecord
            {
                frame = totalFrames,
                time = Time.time,
                pos = rb.position,
                speed = currentSpeed,
                speedDelta = speedDelta,
                linVel = linVel,
                angVel = rb.angularVelocity,
                state = vehicle.currentState.ToString(),
                steer = vehicle.steerInput,
                drift = vehicle.driftHeld,
                boost = vehicle.isBoosting,
                autoAcc = vehicle.autoAccelerate,
                gmState = gm != null ? gm.currentState.ToString() : "N/A",
                chunkCount = tm != null ? tm.activeChunkCount : 0,
                camPos = mainCam != null ? (Vector2)mainCam.transform.position : Vector2.zero,
                lastContact = latestContactInfo,
                contactNormal = latestContactNormal
            };

            ringBuffer[bufferIndex] = record;
            bufferIndex = (bufferIndex + 1) % BUFFER_SIZE;

            // Check for abnormal deceleration or physical velocity arrest:
            // 1. Vehicle controller speed drops abruptly (> 4 m/s in 1 fixed frame) without user braking
            // 2. Physical linear velocity is near zero (< 0.5 m/s) while vehicle.currentSpeed > 5 m/s (physical blockage!)
            // 3. Vehicle speed hits 0 from high speed
            bool abnormalSpeedDrop = (prevSpeed > 6f && currentSpeed < prevSpeed - 3.5f);
            bool physicalBlockage = (currentSpeed > 5f && linVel.magnitude < 1.0f);
            bool unexpectedZero = (prevSpeed > 4f && currentSpeed == 0f);

            if (!eventTriggered && (abnormalSpeedDrop || physicalBlockage || unexpectedZero))
            {
                eventTriggered = true;
                postEventFramesRemaining = 30;

                string reason = abnormalSpeedDrop ? "ABNORMAL SPEED DROP" : (physicalBlockage ? "PHYSICAL BLOCKAGE (linVel ~ 0 vs speed > 5)" : "UNEXPECTED ZERO");
                Debug.LogError($"[DIAGNOSTIC CRITICAL STOP] Reason: {reason} at frame {totalFrames}, pos {rb.position}, speed {currentSpeed:F1} m/s (prev: {prevSpeed:F1}), linVel {linVel}, contact: {latestContactInfo}");

                eventReportBuilder = new StringBuilder();
                eventReportBuilder.AppendLine("================================================================================");
                eventReportBuilder.AppendLine($"   NEON DRIFT — ABNORMAL VEHICLE STOP DIAGNOSTIC DUMP ({reason})");
                eventReportBuilder.AppendLine("================================================================================");
                eventReportBuilder.AppendLine($"Trigger Frame: {totalFrames} | Time: {Time.time:F2}s | Position: {rb.position}");
                eventReportBuilder.AppendLine($"Vehicle State: {vehicle.currentState} | Steer: {vehicle.steerInput:F2} | Drift: {vehicle.driftHeld} | AutoAcc: {vehicle.autoAccelerate}");
                eventReportBuilder.AppendLine($"Vehicle Speed: {currentSpeed:F2} m/s | Prev: {prevSpeed:F2} m/s | Rigidbody2D.linearVelocity: {linVel}");
                eventReportBuilder.AppendLine($"Last Contact: {latestContactInfo} | Normal: {latestContactNormal}");
                eventReportBuilder.AppendLine($"GameManager State: {(gm != null ? gm.currentState.ToString() : "N/A")}");
                eventReportBuilder.AppendLine($"TrackManager Active Chunks: {(tm != null ? tm.activeChunkCount : 0)}");
                eventReportBuilder.AppendLine("\n--- 30 FRAMES PRIOR TO STOP EVENT ---");
                eventReportBuilder.AppendLine("Frame | Time  | Pos (X, Y)      | Speed | linVel (X, Y)   | State    | Steer | Contact");
                eventReportBuilder.AppendLine("--------------------------------------------------------------------------------");

                // Dump prior 30 frames
                for (int i = 0; i < 30; i++)
                {
                    int idx = (bufferIndex - 30 + i + BUFFER_SIZE) % BUFFER_SIZE;
                    var r = ringBuffer[idx];
                    eventReportBuilder.AppendLine($"{r.frame,5} | {r.time,5:F2} | ({r.pos.x,6:F1},{r.pos.y,6:F1}) | {r.speed,5:F1} | ({r.linVel.x,6:F1},{r.linVel.y,6:F1}) | {r.state,-8} | {r.steer,5:F2} | {r.lastContact}");
                }

                eventReportBuilder.AppendLine("\n--- 30 FRAMES POST STOP EVENT ---");
                eventReportBuilder.AppendLine("Frame | Time  | Pos (X, Y)      | Speed | linVel (X, Y)   | State    | Steer | Contact");
                eventReportBuilder.AppendLine("--------------------------------------------------------------------------------");
            }
            else if (eventTriggered && postEventFramesRemaining > 0)
            {
                postEventFramesRemaining--;
                eventReportBuilder.AppendLine($"{record.frame,5} | {record.time,5:F2} | ({record.pos.x,6:F1},{record.pos.y,6:F1}) | {record.speed,5:F1} | ({record.linVel.x,6:F1},{record.linVel.y,6:F1}) | {record.state,-8} | {record.steer,5:F2} | {record.lastContact}");

                if (postEventFramesRemaining == 0)
                {
                    eventReportBuilder.AppendLine("================================================================================");
                    try
                    {
                        File.WriteAllText("Assets/_Project/StopEventDump.txt", eventReportBuilder.ToString());
                        Debug.LogWarning("[DIAGNOSTIC] StopEventDump.txt successfully written to Assets/_Project/StopEventDump.txt");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Failed to write StopEventDump: {ex}");
                    }
                }
            }

            prevSpeed = currentSpeed;
        }
    }
}
