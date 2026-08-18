using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
[System.Serializable]
public struct TimeWindowData
{
	public float timeWindow;
	public float scoreValue;
}
[System.Serializable]
public class NoteData
{
	public float noteTime;
	public int lane;
	public GameObject gameObject;
	public NoteData(float noteTime, int lane, GameObject gameObject)
	{
		this.noteTime = noteTime;
		this.lane = lane;
		this.gameObject = gameObject;
	}
}
public class ManageRhythmGame : MonoBehaviour
{
	private List<float> noteTimes;
	[SerializeField] private float timeForNoteToReachTarget = 2.0f; // Time in seconds for a note to reach the target
	[SerializeField] private List<TimeWindowData> timeWindows; // List of time windows and their corresponding score values
	[SerializeField] private float startYPosition = 5.0f; // Y position where notes start
	[SerializeField] private float targetYPosition = 0.0f; // Y position of the target
	[SerializeField] private GameObject notePrefab;
	[SerializeField] private InputActionReference noteHitActions;
	private Dictionary<int, Vector2> keyLaneMaps = new Dictionary<int, Vector2>() {
 {0, Vector2.left },
		{1, Vector2.up},
		{2, Vector2.down},
		{3, Vector2.right}
	};
	private List<NoteData> noteData = new List<NoteData>();
	private List<GameObject> bottomNotePressIndicators = new List<GameObject>();
	private float timeElapsed = 0.0f;
	private bool gameStarted = false;
	private int currentNoteIndex = 0;
	private Vector2 currentKeyInput;
	public void StartGame(List<float> noteAppearTimes){
		noteHitActions.action.Enable();
		timeElapsed = 0.0f;
		gameStarted = true;
		noteTimes = noteAppearTimes;
		noteTimes.Sort();
		for (int i = 0; i < 4; i++)
		{
			GameObject bottomNotePressIndicator = Instantiate(notePrefab, new Vector3(i * notePrefab.GetComponent<SpriteRenderer>().bounds.size.x, targetYPosition, 0), Quaternion.identity);
			bottomNotePressIndicators.Add(bottomNotePressIndicator);
		}

	}
	private bool laneKeyPressed(int lane)
	{
		Vector2 laneKey = keyLaneMaps[lane];
		return (laneKey.x != 0 && currentKeyInput.x == laneKey.x) || (laneKey.y != 0 && currentKeyInput.y == laneKey.y);
	}
	private void Update()
	{
		if (gameStarted) {
			currentKeyInput = noteHitActions.action.ReadValue<Vector2>();

			while (currentNoteIndex < noteTimes.Count && noteTimes[currentNoteIndex] <= timeElapsed)
			{
				int lane = Random.Range(0, 4);
				GameObject currNotePrefab = Instantiate(notePrefab, new Vector3(lane * notePrefab.GetComponent<SpriteRenderer>().bounds.size.x, startYPosition, 0), Quaternion.identity);
				noteData.Add(new NoteData(0.0f, lane, currNotePrefab));
				currentNoteIndex++;
			}
			
			for(int i = noteData.Count-1; i >= 0; i--)
			{
				NoteData note = noteData[i];
				note.gameObject.transform.position += Vector3.down * (startYPosition - targetYPosition) / timeForNoteToReachTarget * Time.deltaTime;
				note.noteTime += Time.deltaTime;
				if(note.noteTime > timeForNoteToReachTarget + timeWindows[timeWindows.Count - 1].timeWindow)
				{
					Debug.Log($"Missed! Lane: {note.lane}");
					Destroy(note.gameObject);
					noteData.RemoveAt(i);
					continue;
				}
				if (laneKeyPressed(note.lane))
				{
					foreach (TimeWindowData timeWindow in timeWindows)
					{
						if (Mathf.Abs(note.noteTime - timeForNoteToReachTarget) <= timeWindow.timeWindow)
						{
							Debug.Log($"Hit! Lane: {note.lane}, Score: {timeWindow.scoreValue}");
							Destroy(note.gameObject);
							noteData.RemoveAt(i);
							break;
						}
					}
				}
			}
			timeElapsed += Time.deltaTime;
			for(int i = 0; i < bottomNotePressIndicators.Count; i++){
				if(laneKeyPressed(i))
				{
					bottomNotePressIndicators[i].GetComponent<SpriteRenderer>().color = Color.green;
				}
				else
				{
					bottomNotePressIndicators[i].GetComponent<SpriteRenderer>().color = Color.white;
				}
			}
		}
	}
}
