using UnityEngine;

public class Raise : MonoBehaviour
{
    public float speed = 1.0f;
    public float distance = 3f;
    private Vector3 _startPosition;
    private Vector3 _endPosition;
    private float _startTime;
    private float _journeyLength;

    void Start()
    {
        _startTime = Time.time;
        _startPosition = transform.position;
        _endPosition = _startPosition + Vector3.up * distance;
        _journeyLength = Vector3.Distance(_startPosition, _endPosition);
    }

    void Update()
    {
        var distCovered = (Time.time - _startTime) * speed;
        var fractionOfJourney = distCovered / _journeyLength;
        transform.position = Vector3.Lerp(_startPosition, _endPosition, fractionOfJourney);
    }
}
