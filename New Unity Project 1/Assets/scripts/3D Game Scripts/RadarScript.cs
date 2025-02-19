﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadarScript : MonoBehaviour
{
    public float radarDistance = 20, blipSize = 15;
    public bool usePlayerDirection = true;
    public Transform player;
    public GameObject AsteroidBlipPrefab;
    public GameObject MissileBlipPrefab;
    //public string redBlipTag = "Enemy", greenBlipTag = "NPC";

    private float radarWidth, radarHeight, blipWidth, blipHeight;

    void Start()
    {
        radarWidth = GetComponent<RectTransform>().rect.width;
        radarHeight = GetComponent<RectTransform>().rect.height;
        blipHeight = radarHeight * blipSize / 100;
        blipWidth = radarWidth * blipSize / 100;
    }

    void Update()
    {
        RemoveAllBlips();
        DisplayBlips();
    }

    private void DisplayBlips()
    {
        Vector3 playerPos = player.position;
        GameObject[] targets = Camera.main.GetComponent<GameManager>().GetAllAsteroids();

        foreach (GameObject target in targets)
        {
            Vector3 targetPos = target.transform.position;
            float distanceToTarget = Vector3.Distance(targetPos, playerPos);

            if (distanceToTarget <= radarDistance)
            {

                Vector3 normalisedTargetPosition = NormalisedPosition(playerPos, targetPos);
                Vector2 blipPosition = CalculateBlipPosition(normalisedTargetPosition);
                if (target.TryGetComponent<AsteroidManager>(out AsteroidManager a))
                {
                    DrawBlip(blipPosition, AsteroidBlipPrefab);
                }
                else
                {
                    DrawBlip(blipPosition, MissileBlipPrefab);
                }
            }
        }
    }

    private void RemoveAllBlips()
    {
        GameObject[] blips = GameObject.FindGameObjectsWithTag("RadarBlip");
        foreach (GameObject blip in blips)
            Destroy(blip);
    }

    private Vector3 NormalisedPosition(Vector3 playerPos, Vector3 targetPos)
    {
        float normalisedTargetX = (targetPos.x - playerPos.x) / radarDistance;
        float normalisedTargetZ = (targetPos.z - playerPos.z) / radarDistance;

        return new Vector3(normalisedTargetX, 0, normalisedTargetZ);
    }

    private Vector2 CalculateBlipPosition(Vector3 targetPos)
    {
        // find the angle from the player to the target.
        float angleToTarget = Mathf.Atan2(targetPos.x, targetPos.z) * Mathf.Rad2Deg;

        // The direction the player is facing.
        float anglePlayer = usePlayerDirection ? player.eulerAngles.y : 0;

        // Subtract the player angle, to get the relative angle to the object. Subtract 90
        // so 0 degrees (the same direction as the player) is Up.
        float angleRadarDegrees = angleToTarget - anglePlayer - 90;

        // Calculate the xy position given the angle and the distance.
        float normalisedDistanceToTarget = targetPos.magnitude;
        float angleRadians = angleRadarDegrees * Mathf.Deg2Rad;
        float blipX = normalisedDistanceToTarget * Mathf.Cos(angleRadians);
        float blipY = normalisedDistanceToTarget * Mathf.Sin(angleRadians);

        // Scale the blip position according to the radar size.
        blipX *= radarWidth * .5f;
        blipY *= radarHeight * .5f;

        // Offset the blip position relative to the radar center
        blipX += (radarWidth * .5f) - blipWidth * .5f;
        blipY += (radarHeight * .5f) - blipHeight * .5f;

        return new Vector2(blipX, blipY);
    }

    private void DrawBlip(Vector2 pos, GameObject blipPrefab)
    {
        GameObject blip = (GameObject)Instantiate(blipPrefab);
        blip.transform.SetParent(transform);
        RectTransform rt = blip.GetComponent<RectTransform>();
        rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, pos.x, blipWidth);
        rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, pos.y, blipHeight);
    }
}