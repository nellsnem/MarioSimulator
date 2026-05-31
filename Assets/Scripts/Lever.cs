using UnityEngine;

public class Lever : MonoBehaviour
{
	// ==========================================
	// 1. PUBLIC FIELDS
	// ==========================================
	[Header("Settings")]
	[Tooltip("RedCoin = Mario 1 only, GreenCoin = Mario 2 only")]
	public string coinTag    = "RedCoin";
	public int requiredCoins = 3;

	[Header("Linked Flowers")]
	public FlowerBarrier[] linkedFlowers;

	[Header("Sprites")]
	public Sprite spriteIdle;
	public Sprite spriteActivated;

	// ==========================================
	// 2. PRIVATE FIELDS
	// ==========================================
	private bool _isActivated = false;
	private SpriteRenderer _spriteRenderer;

	// ==========================================
	// 3. MONOBEHAVIOUR METHODS
	// ==========================================
	private void Awake()
	{
		_spriteRenderer = GetComponent<SpriteRenderer>();

		if (_spriteRenderer != null && spriteIdle != null)
		{
			_spriteRenderer.sprite = spriteIdle;
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (_isActivated)                return;
		if (!other.CompareTag("Player")) return;

		PlayerMovement player = other.GetComponent<PlayerMovement>();
		if (player == null)              return;

		// Check if the correct player triggered the lever
		if (!IsCorrectPlayer(player))    return;

		// Check if the player has enough coins
		if (!HasEnoughCoins())           return;

		Activate();
	}

	// ==========================================
	// 4. PRIVATE METHODS
	// ==========================================
	private bool IsCorrectPlayer(PlayerMovement player)
	{
		if (coinTag == "RedCoin"   && player.playerIndex == 1) return true;
		if (coinTag == "GreenCoin" && player.playerIndex == 2) return true;
		return false;
	}

	private bool HasEnoughCoins()
	{
		if (coinTag == "RedCoin")
		{
			return GameManager.Instance.redCoins >= requiredCoins;
		}

		if (coinTag == "GreenCoin")
		{
			return GameManager.Instance.greenCoins >= requiredCoins;
		}

		return false;
	}

	private void Activate()
	{
		_isActivated = true;

		if (_spriteRenderer != null && spriteActivated != null)
		{
			_spriteRenderer.sprite = spriteActivated;
		}

		foreach (FlowerBarrier flower in linkedFlowers)
		{
			if (flower != null)
			{
				flower.Open();
			}
		}
	}
}