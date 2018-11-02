using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MUPS.UI
{
	/// <summary>
	/// Describes a modal button's text and on-click event handler.
	/// </summary>
	public class ButtonDescriptor
	{
		public UnityAction[] Actions;
		public string Text;

		/// <summary>
		/// Creates a new button descriptor with the given name and event handlers.
		/// </summary>
		/// <param name="text">The button's label.</param>
		/// <param name="actions">Actions that are executed on click.</param>
		public ButtonDescriptor(string text, params UnityAction[] actions)
		{
			Actions = actions;
			Text = text;
		}

		/// <summary>
		/// Creates a new button descriptor with the given name and no actions (clicking the button still closes the modal).
		/// </summary>
		/// <param name="text">The button's label.</param>
		public ButtonDescriptor(string text)
		{
			Text = text;
			Actions = new UnityAction[0];
		}
	}

	public class ModalBehaviour : MonoBehaviour
	{
		public Text titleText;
		public Text messageText;
		public GameObject background;
		public RectTransform window;
		public RectTransform buttonContainer;
		public Vector2 defaultSize = new Vector2(400, 250);

		public static ModalBehaviour Instance { get; private set; }

		private GameObject _buttonPrefab;

		private void Awake()
		{
			_buttonPrefab = Resources.Load<GameObject>("Prefabs/ModalButton");
			if (Instance == null)
			{
				Instance = this;
			}
			else if (Instance != this)
			{
				Destroy(Instance);
				Instance = this;
			}

			Close();
		}

		/// <summary>
		/// Close the modal window.
		/// </summary>
		public void Close()
		{
			foreach (Transform tr in buttonContainer)
				Destroy(tr.gameObject);

			background.SetActive(false);
		}

		/// <summary>
		/// Open a modal window.
		/// </summary>
		/// <param name="title">Title row text.</param>
		/// <param name="content">Message content.</param>
		/// <param name="buttons">Descriptors that define the window's buttons.</param>
		public void Show(string title, string content, params ButtonDescriptor[] buttons)
		{
			Show(new Vector2(), defaultSize, title, content, buttons);
		}

		/// <summary>
		/// Open a modal window.
		/// </summary>
		/// <param name="position">Position relative to the center of the screen.</param>
		/// <param name="size">Modal window size.</param>
		/// <param name="title">Title row text.</param>
		/// <param name="content">Message content.</param>
		/// <param name="buttons">Descriptors that define the window's buttons.</param>
		public void Show(Vector2 position, Vector2 size, string title, string content, params ButtonDescriptor[] buttons)
		{
			window.sizeDelta = size;
			window.anchoredPosition = position;

			if (titleText != null)
				titleText.text = title;
			if (messageText != null)
				messageText.text = content;

			foreach (ButtonDescriptor desc in buttons)
			{
				GameObject o = Instantiate<GameObject>(_buttonPrefab, buttonContainer);
				o.GetComponentInChildren<Text>().text = desc.Text;
				Button b = o.GetComponent<Button>();
				b.onClick.RemoveAllListeners();
				foreach (UnityAction a in desc.Actions)
					b.onClick.AddListener(a);
				b.onClick.AddListener(Close);
			}

			background.SetActive(true);
		}
	}
}