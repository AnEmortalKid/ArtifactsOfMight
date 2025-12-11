# Color Tuner


Sample usage, basically create a parent that the UI is going to stretch into,
then position that where you want.
```cs
var tunerUI = new GameObject("Tuner", typeof(RectTransform));
FactoryUtils.ParentToRectTransform(tunerUI, parent);
   var tunerRect = tunerUI.GetComponent<RectTransform>();

// Top-left fixed rect
tunerRect.anchorMin = new Vector2(0f, 1f);
tunerRect.anchorMax = new Vector2(0f, 1f);
tunerRect.pivot = new Vector2(0f, 1f);
tunerRect.sizeDelta = new Vector2(300,200);
tunerRect.anchoredPosition = new Vector2(12, -12);

// Don't let parent layout groups move/resize it
var le = tunerUI.AddComponent<LayoutElement>();
le.ignoreLayout = true;

// then attach the image
ColorTunerUI.AttachTunerUI(tunerRect, glassPanelBackgroundImage);
```
