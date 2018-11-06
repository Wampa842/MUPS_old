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
		/// <summary>
		/// A collection of preset colors.
		/// </summary>
		public static class ColorPresets
		{
			public static ColorBlock Default { get { return ColorBlock.defaultColorBlock; } }
			public static ColorBlock Red { get { return new ColorBlock { normalColor = new Color(0.8f, 0.2f, 0.2f), highlightedColor = new Color(0.85f, 0.25f, 0.25f), pressedColor = new Color(0.7f, 0.15f, 0.15f), disabledColor = new Color(0.7f, 0.2f, 0.2f), colorMultiplier = 1, fadeDuration = 0 }; } }
			public static ColorBlock Green { get { return new ColorBlock { normalColor = new Color(0.2f, 0.8f, 0.2f), highlightedColor = new Color(0.25f, 0.85f, 0.25f), pressedColor = new Color(0.15f, 0.7f, 0.15f), disabledColor = new Color(0.2f, 0.7f, 0.2f), colorMultiplier = 1, fadeDuration = 0 }; } }
			public static ColorBlock Blue { get { return new ColorBlock { normalColor = new Color(0.2f, 0.2f, 0.8f), highlightedColor = new Color(0.25f, 0.25f, 0.85f), pressedColor = new Color(0.15f, 0.15f, 0.7f), disabledColor = new Color(0.2f, 0.2f, 0.7f), colorMultiplier = 1, fadeDuration = 0 }; } }
			public static ColorBlock LightBlue { get { return new ColorBlock { normalColor = new Color(0.675f, 0.825f, 1.0f), highlightedColor = new Color(0.778f, 0.894f, 1.0f), pressedColor = new Color(0.416f, 0.615f, 0.84f), disabledColor = new Color(0.54f, 0.62f, 0.72f), colorMultiplier = 1, fadeDuration = 0 }; } }
		}
		public static ButtonDescriptor DismissPreset { get { return new ButtonDescriptor("Dismiss"); } }

		public UnityAction[] Actions { get; }
		public string Text { get; }
		public ColorBlock Colors { get; }
		private bool _colorsDefined = false;
		public bool ColorsDefined { get { return _colorsDefined; } }
		public bool CloseOnClick { get; }

		/// <summary>
		/// Creates a new button descriptor with the given name and event handlers.
		/// </summary>
		/// <param name="text">The button's label.</param>
		/// <param name="closeOnClick">If true, clicking the button will close the modal window.</param>
		/// <param name="actions">Actions that are executed on click.</param>
		public ButtonDescriptor(string text, bool closeOnClick, params UnityAction[] actions)
		{
			Text = text;
			CloseOnClick = closeOnClick;
			Actions = actions;
		}

		/// <summary>
		/// Creates a new button descriptor with the given name and event handlers.
		/// </summary>
		/// <param name="text">The button's label.</param>
		/// <param name="actions">Actions that are executed on click.</param>
		public ButtonDescriptor(string text, params UnityAction[] actions) : this(text, true, actions)
		{

		}

		/// <summary>
		/// Creates a new button descriptor with the given name, colors, and event handlers.
		/// </summary>
		/// <param name="text">The button's label.</param>
		/// <param name="colors">The ColorBlock that defines the button's color.</param>
		/// <param name="closeOnClick">If true, clicking the button will close the modal window.</param>
		/// <param name="actions">Actions that are executed on click.</param>
		public ButtonDescriptor(string text, ColorBlock colors, bool closeOnClick, params UnityAction[] actions)
		{
			Text = text;
			Colors = colors;
			_colorsDefined = true;
			Actions = actions;
			CloseOnClick = closeOnClick;
		}

		/// <summary>
		/// Creates a new button descriptor with the given name, colors, and event handlers.
		/// </summary>
		/// <param name="text">The button's label.</param>
		/// <param name="colors">The ColorBlock that defines the button's color.</param>
		/// <param name="actions">Actions that are executed on click.</param>
		public ButtonDescriptor(string text, ColorBlock colors, params UnityAction[] actions) : this(text, colors, true, actions)
		{

		}
	}

	/// <summary>
	/// Wrapper class for ModalBehaviour.Instance
	/// </summary>
	public static class Modal
	{
		/// <summary>
		/// Indicates if the modal is currently open.
		/// </summary>
		public static bool IsOpen { get { return ModalBehaviour.Instance.IsOpen; } }

		/// <summary>
		/// Close the modal window.
		/// </summary>
		public static void Close()
		{
			foreach (Transform tr in ModalBehaviour.Instance.buttonContainer)
				GameObject.Destroy(tr.gameObject);

			ModalBehaviour.Instance.background.SetActive(false);
		}

		/// <summary>
		/// Open a modal window.
		/// </summary>
		/// <param name="title">Title row text.</param>
		/// <param name="content">Message content.</param>
		/// <param name="buttons">Descriptors that define the window's buttons.</param>
		public static void Show(string title, string content, params ButtonDescriptor[] buttons)
		{
			ModalBehaviour.Instance.Show(title, content, buttons);
		}

		/// <summary>
		/// Open a modal window.
		/// </summary>
		/// <param name="position">Position relative to the center of the screen.</param>
		/// <param name="size">Modal window size.</param>
		/// <param name="title">Title row text.</param>
		/// <param name="content">Message content.</param>
		/// <param name="buttons">Descriptors that define the window's buttons.</param>
		public static void Show(Vector2 position, Vector2 size, string title, string content, params ButtonDescriptor[] buttons)
		{
			ModalBehaviour.Instance.Show(position, size, title, content, buttons);
		}

		/// <summary>
		/// Open a modal window.
		/// </summary>
		/// <param name="width">Modal window width.</param>
		/// <param name="height">Modal window height.</param>
		/// <param name="title">Title row text.</param>
		/// <param name="content">Message content.</param>
		/// <param name="buttons">Descriptors that define the window's buttons.</param>
		public static void Show(float width, float height, string title, string content, params ButtonDescriptor[] buttons)
		{
			Show(new Vector2(), new Vector2(width, height), title, content, buttons);
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
		public GameObject ButtonPrefab { get; protected set; }

		public static ModalBehaviour Instance { get; private set; }

		private void Awake()
		{
			ButtonPrefab = Resources.Load<GameObject>("Prefabs/GUI/ModalWindow/ModalButton");
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
		/// Indicates if the modal is currently open.
		/// </summary>
		public bool IsOpen
		{
			get
			{
				return background.activeInHierarchy;
			}
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
			if (IsOpen)
				return;
			window.sizeDelta = size;
			window.anchoredPosition = position;

			if (titleText != null)
				titleText.text = title;
			if (messageText != null)
				messageText.text = content;

			foreach (ButtonDescriptor desc in buttons)
			{
				GameObject o = Instantiate<GameObject>(ButtonPrefab, buttonContainer);
				o.GetComponentInChildren<Text>().text = desc.Text;
				Button b = o.GetComponent<Button>();
				b.onClick.RemoveAllListeners();
				foreach (UnityAction a in desc.Actions)
					b.onClick.AddListener(a);
				if (desc.CloseOnClick)
					b.onClick.AddListener(Close);
				if (desc.ColorsDefined)
					b.colors = desc.Colors;
			}

			background.SetActive(true);
		}

		/// <summary>
		/// Open a modal window.
		/// </summary>
		/// <param name="width">Modal window width.</param>
		/// <param name="height">Modal window height.</param>
		/// <param name="title">Title row text.</param>
		/// <param name="content">Message content.</param>
		/// <param name="buttons">Descriptors that define the window's buttons.</param>
		public void Show(float width, float height, string title, string content, params ButtonDescriptor[] buttons)
		{
			Show(new Vector2(), new Vector2(width, height), title, content, buttons);
		}
	}
}