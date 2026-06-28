using UnityEngine;
using System.Collections;

public class SlugDryingState : ISlugState
{
    private readonly SlugAI _own;
    private float _lightTimer;

    private Vector3 _origScale;
    private bool _scaleInitialized;
    private bool _skipFirstFrame;

    public SlugDryingState(SlugAI own) => _own = own;

    public void Enter()
    {
        if (!_scaleInitialized)
        {
            _origScale = _own.transform.localScale;
            _scaleInitialized = true;
        }
        _skipFirstFrame = true;
    }

    public void Update() 
    {
        if (_skipFirstFrame)
        {
            _skipFirstFrame = false;
            return;
        }

        if (_own.lightKillTime <= 0f) return; 

        _lightTimer += Time.deltaTime;

        float time = Mathf.Clamp01(_lightTimer / _own.lightKillTime);


        _own.transform.localScale = Vector3.Lerp(_origScale, _origScale * 0.3f, time);

        Color slugColor = _own.slugSpr.color;
        slugColor.a = 1f - time;
        _own.slugSpr.color = slugColor;
        SlugAI.SetAlpha(_own.crustSpr, time);

        if (_lightTimer >= _own.lightKillTime)
        {
            _own.slugSpr.transform.localPosition = Vector3.zero;
            _own.TransitionTo(_own.GetDead());
        }
    }

    public void Exit()
    {
        if (_own.slugSpr != null) _own.slugSpr.transform.localPosition = Vector3.zero;
    }

    public void ResetProgress()
    {
        _lightTimer = 0f;
        if (_scaleInitialized) _own.transform.localScale = _origScale;
        SlugAI.SetAlpha(_own.slugSpr, 0f);
        Color color = _own.slugSpr.color;
        color.a = 1f;
        _own.slugSpr.color = color;
    }
}
