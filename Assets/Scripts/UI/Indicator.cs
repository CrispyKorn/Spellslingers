using UnityEngine;
using TMPro;
using System;

[Serializable]
public class Indicator
{
    public int AttackCounter { get => _attackCounter; set => _attackCounter = value; }
    public int DefenceCounter { get => _defenceCounter; set => _defenceCounter = value; }
    public int SpecialAttackCounter { get => _specialAttackCounter; set => _specialAttackCounter = value; }
    public int SpecialDefenceCounter { get => _specialDefenceCounter; set => _specialDefenceCounter = value; }
    public Animator AttackCounterAnimator { get => _attackCounterAnimator; }
    public Animator DefenceCounterAnimator { get => _defenceCounterAnimator; }
    public Animator SpecialAttackCounterAnimator { get => _specialAttackCounterAnimator; }
    public Animator SpecialDefenceCounterAnimator { get => _specialDefenceCounterAnimator; }

    [SerializeField] private TextMeshProUGUI _attackCounterText;
    [SerializeField] private TextMeshProUGUI _defenceCounterText;
    [SerializeField] private TextMeshProUGUI _specialAttackCounterText;
    [SerializeField] private TextMeshProUGUI _specialDefenceCounterText;
    private Animator _attackCounterAnimator;
    private Animator _defenceCounterAnimator;
    private Animator _specialAttackCounterAnimator;
    private Animator _specialDefenceCounterAnimator;

    private int _attackCounter;
    private int _defenceCounter;
    private int _specialAttackCounter;
    private int _specialDefenceCounter;

    public void Initialize()
    {
        _attackCounterAnimator = _attackCounterText.GetComponentInParent<Animator>();
        _defenceCounterAnimator = _defenceCounterText.GetComponentInParent<Animator>();
        _specialAttackCounterAnimator = _specialAttackCounterText.GetComponentInParent<Animator>();
        _specialDefenceCounterAnimator = _specialDefenceCounterText.GetComponentInParent<Animator>();
    }

    public void UpdateText()
    {
        _attackCounterText.text = _attackCounter.ToString();
        _defenceCounterText.text = _defenceCounter.ToString();
        _specialAttackCounterText.text = _specialAttackCounter.ToString();
        _specialDefenceCounterText.text = _specialDefenceCounter.ToString();

        _attackCounterText.gameObject.SetActive(_attackCounter > 0);
        _defenceCounterText.gameObject.SetActive(_defenceCounter > 0);
        _specialAttackCounterText.gameObject.SetActive(_specialAttackCounter > 0);
        _specialDefenceCounterText.gameObject.SetActive(_specialDefenceCounter > 0);
    }

    public void ResetCounters()
    {
        _attackCounter = 0;
        _defenceCounter = 0;
        _specialAttackCounter = 0;
        _specialDefenceCounter = 0;

        _attackCounterText.gameObject.SetActive(false);
        _defenceCounterText.gameObject.SetActive(false);
        _specialAttackCounterText.gameObject.SetActive(false);
        _specialDefenceCounterText.gameObject.SetActive(false);

        UpdateText();
    }
}
