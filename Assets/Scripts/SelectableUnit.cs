using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer))]
public class SelectableUnit : MonoBehaviour
{
    private SpriteRenderer spriteRenderer = null!;

    [Header("Config")]
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private Sprite unitPortrait;
    [SerializeField] private CommandType[] availableCommands;

    [Header("State")]
    [SerializeField] private Color originalColor;

    public virtual void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    public HashSet<CommandType> GetAvailableCommands()
    {
        return new HashSet<CommandType>(availableCommands);
    }

    public virtual void ShowSelectedEffect()
    {
        spriteRenderer.color = highlightColor;
    }

    public virtual void RemoveSelectedEffect()
    {
        spriteRenderer.color = originalColor;
    }

    public Sprite GetUnitPortrait()
    {
        return unitPortrait;
    }
}
