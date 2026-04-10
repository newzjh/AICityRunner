using UnityEngine;

public enum DynamicStreetItemType
{
    ScorePickup,
    Obstacle,
    LifePickup,
    SpeedPickup,
    CheckInPickup,
    FoodStall,
    ArcadeSign,
    SharedBikeSpot,
    FlowerMarket,
    TransitStop
}

public class DynamicStreetRuntimeItem : MonoBehaviour
{
    public DynamicStreetItemType ItemType;
    public string ItemLabel;
    public string CityName;
    public int ScoreValue = 1;
    public int HealthDelta = 0;
    public float SpeedDelta = 2f;
    public float SpeedDuration = 4f;
    public bool EnableBob = true;
    public bool EnableSpin = true;
    public float BobAmplitude = 0.18f;
    public float BobFrequency = 2f;
    public float SpinSpeed = 90f;

    private Vector3 _startLocalPosition;
    private bool _consumed;

    private void Awake()
    {
        _startLocalPosition = transform.localPosition;
    }

    private void Update()
    {
        if (EnableBob)
        {
            float yOffset = Mathf.Sin(Time.time * BobFrequency) * BobAmplitude;
            transform.localPosition = _startLocalPosition + Vector3.up * yOffset;
        }

        if (EnableSpin)
        {
            transform.Rotate(Vector3.forward, SpinSpeed * Time.deltaTime, Space.Self);
        }
    }

    public bool TryConsume()
    {
        if (_consumed)
        {
            return false;
        }

        _consumed = true;
        return true;
    }
}
