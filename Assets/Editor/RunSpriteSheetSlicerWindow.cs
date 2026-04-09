using System;
using UnityEditor;
using UnityEngine;

internal readonly struct RunSpriteSheetSliceSettings
{
    public RunSpriteSheetSliceSettings(
        Texture2D texture,
        RectInt cropRect,
        int rows,
        int cols,
        int frameSpacing,
        int paddingLeft,
        int paddingRight,
        int paddingTop,
        int paddingBottom,
        int pixelsPerUnit)
    {
        Texture = texture;
        CropRect = cropRect;
        Rows = rows;
        Cols = cols;
        FrameSpacing = frameSpacing;
        PaddingLeft = paddingLeft;
        PaddingRight = paddingRight;
        PaddingTop = paddingTop;
        PaddingBottom = paddingBottom;
        PixelsPerUnit = pixelsPerUnit;
    }

    public Texture2D Texture { get; }
    public RectInt CropRect { get; }
    public int Rows { get; }
    public int Cols { get; }
    public int FrameSpacing { get; }
    public int PaddingLeft { get; }
    public int PaddingRight { get; }
    public int PaddingTop { get; }
    public int PaddingBottom { get; }
    public int PixelsPerUnit { get; }
}

internal sealed class RunSpriteSheetSlicerWindow : EditorWindow
{
    private enum DragMode
    {
        None,
        Move,
        Left,
        Right,
        Top,
        Bottom,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    private const float HandleSize = 10f;
    private const int MinSelectionSize = 8;

    private Texture2D _texture;
    private RectInt _cropRect;
    private int _rows;
    private int _cols;
    private int _frameSpacing;
    private int _paddingLeft;
    private int _paddingRight;
    private int _paddingTop;
    private int _paddingBottom;
    private int _pixelsPerUnit;

    private Action<RunSpriteSheetSliceSettings> _onConfirm;

    private Rect _lastTextureDrawRect;
    private DragMode _dragMode;
    private Vector2Int _dragStartMouseTex;
    private RectInt _dragStartCropRect;

    public static void Show(
        Texture2D texture,
        int rows,
        int cols,
        int frameSpacing,
        int paddingLeft,
        int paddingRight,
        int paddingTop,
        int paddingBottom,
        int pixelsPerUnit,
        Action<RunSpriteSheetSliceSettings> onConfirm)
    {
        RunSpriteSheetSlicerWindow window = GetWindow<RunSpriteSheetSlicerWindow>(true, "跑酷切图", true);
        window.minSize = new Vector2(780, 560);
        window._texture = texture;
        window._rows = Mathf.Max(1, rows);
        window._cols = Mathf.Max(1, cols);
        window._frameSpacing = Mathf.Max(0, frameSpacing);
        window._paddingLeft = Mathf.Max(0, paddingLeft);
        window._paddingRight = Mathf.Max(0, paddingRight);
        window._paddingTop = Mathf.Max(0, paddingTop);
        window._paddingBottom = Mathf.Max(0, paddingBottom);
        window._pixelsPerUnit = Mathf.Max(1, pixelsPerUnit);
        window._onConfirm = onConfirm;

        if (texture != null)
        {
            window._cropRect = new RectInt(0, 0, texture.width, texture.height);
        }

        window.ShowUtility();
    }

    private void OnGUI()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            DrawLeftPanel();
            DrawRightPanel();
        }

        HandleInput();
    }

    private void DrawLeftPanel()
    {
        using (new EditorGUILayout.VerticalScope(GUILayout.Width(300)))
        {
            EditorGUI.BeginChangeCheck();
            _texture = (Texture2D)EditorGUILayout.ObjectField("源图", _texture, typeof(Texture2D), false);
            if (EditorGUI.EndChangeCheck())
            {
                if (_texture != null)
                {
                    _cropRect = new RectInt(0, 0, _texture.width, _texture.height);
                }
            }

            using (new EditorGUI.DisabledScope(_texture == null))
            {
                _rows = Mathf.Max(1, EditorGUILayout.IntField("行数", _rows));
                _cols = Mathf.Max(1, EditorGUILayout.IntField("列数", _cols));
                _frameSpacing = Mathf.Max(0, EditorGUILayout.IntField("格间距(px)", _frameSpacing));
                _paddingLeft = Mathf.Max(0, EditorGUILayout.IntField("内缩左(px)", _paddingLeft));
                _paddingRight = Mathf.Max(0, EditorGUILayout.IntField("内缩右(px)", _paddingRight));
                _paddingTop = Mathf.Max(0, EditorGUILayout.IntField("内缩上(px)", _paddingTop));
                _paddingBottom = Mathf.Max(0, EditorGUILayout.IntField("内缩下(px)", _paddingBottom));
                _pixelsPerUnit = Mathf.Max(1, EditorGUILayout.IntField("PPU", _pixelsPerUnit));

                EditorGUILayout.Space(6);

                RectInt newRect = _cropRect;
                newRect.x = EditorGUILayout.IntField("裁剪X", newRect.x);
                newRect.y = EditorGUILayout.IntField("裁剪Y", newRect.y);
                newRect.width = EditorGUILayout.IntField("裁剪W", newRect.width);
                newRect.height = EditorGUILayout.IntField("裁剪H", newRect.height);
                if (newRect != _cropRect)
                {
                    _cropRect = ClampRectToTexture(newRect, _texture);
                }

                EditorGUILayout.Space(6);

                (int cellW, int cellH) = ComputeCellSize(_cropRect, _rows, _cols, _frameSpacing);
                EditorGUILayout.LabelField("单元尺寸(px)", $"{cellW} x {cellH}");
                EditorGUILayout.LabelField("裁剪尺寸(px)", $"{_cropRect.width} x {_cropRect.height}");

                bool valid = IsValidForSlicing(_texture, _cropRect, _rows, _cols, _frameSpacing, _paddingLeft, _paddingRight, _paddingTop, _paddingBottom);
                using (new EditorGUI.DisabledScope(!valid))
                {
                    if (GUILayout.Button("确定"))
                    {
                        RunSpriteSheetSliceSettings settings = new RunSpriteSheetSliceSettings(
                            _texture,
                            _cropRect,
                            _rows,
                            _cols,
                            _frameSpacing,
                            _paddingLeft,
                            _paddingRight,
                            _paddingTop,
                            _paddingBottom,
                            _pixelsPerUnit
                        );
                        Close();
                        _onConfirm?.Invoke(settings);
                    }
                }

                if (GUILayout.Button("取消"))
                {
                    Close();
                }

                if (!valid)
                {
                    EditorGUILayout.HelpBox("当前参数无法有效切分，请检查裁剪范围、行列数、格间距、内缩。", MessageType.Warning);
                }
            }
        }
    }

    private void DrawRightPanel()
    {
        using (new EditorGUILayout.VerticalScope())
        {
            Rect previewRect = GUILayoutUtility.GetRect(10, 100000, 10, 100000, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            EditorGUI.DrawRect(previewRect, new Color(0.12f, 0.12f, 0.12f, 1f));

            if (_texture == null)
            {
                GUI.Label(previewRect, "请选择一张 Texture2D", EditorStyles.centeredGreyMiniLabel);
                _lastTextureDrawRect = Rect.zero;
                return;
            }

            Rect texRect = GetTextureDrawRect(previewRect, _texture.width, _texture.height);
            GUI.DrawTexture(texRect, _texture, ScaleMode.ScaleToFit);
            _lastTextureDrawRect = texRect;

            DrawOverlay(texRect);
        }
    }

    private void DrawOverlay(Rect texRect)
    {
        Rect selectionGuiRect = TextureRectToGuiRect(_cropRect, texRect, _texture.width, _texture.height);

        Handles.BeginGUI();
        Color outline = new Color(0.2f, 0.8f, 1f, 1f);
        Color fill = new Color(0.2f, 0.8f, 1f, 0.08f);
        Handles.DrawSolidRectangleWithOutline(selectionGuiRect, fill, outline);

        if (IsValidForSlicing(_texture, _cropRect, _rows, _cols, _frameSpacing, _paddingLeft, _paddingRight, _paddingTop, _paddingBottom))
        {
            DrawCellRects(texRect);
            DrawGrid(selectionGuiRect, texRect);
        }

        DrawHandles(selectionGuiRect);
        Handles.EndGUI();
    }

    private void DrawCellRects(Rect texRect)
    {
        (int cellW, int cellH) = ComputeCellSize(_cropRect, _rows, _cols, _frameSpacing);
        if (cellW <= 0 || cellH <= 0)
        {
            return;
        }

        int innerW = cellW - _paddingLeft - _paddingRight;
        int innerH = cellH - _paddingTop - _paddingBottom;
        if (innerW <= 0 || innerH <= 0)
        {
            return;
        }

        Color cellOutline = new Color(1f, 0.9f, 0.1f, 1f);
        Color cellFill = new Color(1f, 0.9f, 0.1f, 0.03f);

        for (int row = 0; row < _rows; row++)
        {
            for (int col = 0; col < _cols; col++)
            {
                int cellX = _cropRect.x + col * (cellW + _frameSpacing) + _paddingLeft;
                int cellY = _cropRect.y + (_rows - 1 - row) * (cellH + _frameSpacing) + _paddingBottom;
                RectInt cellTexRect = new RectInt(cellX, cellY, innerW, innerH);
                Rect cellGuiRect = TextureRectToGuiRect(cellTexRect, texRect, _texture.width, _texture.height);
                Handles.DrawSolidRectangleWithOutline(cellGuiRect, cellFill, cellOutline);
            }
        }
    }

    private void DrawGrid(Rect selectionGuiRect, Rect texRect)
    {
        (int cellW, int cellH) = ComputeCellSize(_cropRect, _rows, _cols, _frameSpacing);
        if (cellW <= 0 || cellH <= 0)
        {
            return;
        }

        float scaleX = texRect.width / _texture.width;
        float scaleY = texRect.height / _texture.height;

        Vector2 cropTopLeftGui = new Vector2(selectionGuiRect.xMin, selectionGuiRect.yMin);

        Handles.color = new Color(1f, 1f, 1f, 0.35f);

        for (int c = 1; c < _cols; c++)
        {
            float x = cropTopLeftGui.x + (c * cellW + (c - 1) * _frameSpacing) * scaleX;
            Handles.DrawLine(new Vector3(x, selectionGuiRect.yMin), new Vector3(x, selectionGuiRect.yMax));
        }

        for (int r = 1; r < _rows; r++)
        {
            float y = cropTopLeftGui.y + (r * cellH + (r - 1) * _frameSpacing) * scaleY;
            Handles.DrawLine(new Vector3(selectionGuiRect.xMin, y), new Vector3(selectionGuiRect.xMax, y));
        }
    }

    private void DrawHandles(Rect selectionGuiRect)
    {
        Rect[] handleRects =
        {
            GetHandleRect(selectionGuiRect, DragMode.TopLeft),
            GetHandleRect(selectionGuiRect, DragMode.TopRight),
            GetHandleRect(selectionGuiRect, DragMode.BottomLeft),
            GetHandleRect(selectionGuiRect, DragMode.BottomRight),
            GetHandleRect(selectionGuiRect, DragMode.Left),
            GetHandleRect(selectionGuiRect, DragMode.Right),
            GetHandleRect(selectionGuiRect, DragMode.Top),
            GetHandleRect(selectionGuiRect, DragMode.Bottom)
        };

        Color handleColor = new Color(0.2f, 0.8f, 1f, 0.9f);
        foreach (Rect r in handleRects)
        {
            Handles.DrawSolidRectangleWithOutline(r, handleColor, new Color(0f, 0f, 0f, 0.8f));
        }
    }

    private void HandleInput()
    {
        if (_texture == null || _lastTextureDrawRect == Rect.zero)
        {
            return;
        }

        Event e = Event.current;
        if (e == null)
        {
            return;
        }

        Rect selectionGuiRect = TextureRectToGuiRect(_cropRect, _lastTextureDrawRect, _texture.width, _texture.height);

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            DragMode hit = HitTest(selectionGuiRect, e.mousePosition);
            if (hit != DragMode.None)
            {
                _dragMode = hit;
                _dragStartMouseTex = GuiToTexturePointInt(e.mousePosition, _lastTextureDrawRect, _texture.width, _texture.height);
                _dragStartCropRect = _cropRect;
                GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);
                e.Use();
                return;
            }
        }

        if (e.type == EventType.MouseDrag && e.button == 0 && _dragMode != DragMode.None)
        {
            Vector2Int mouseTex = GuiToTexturePointInt(e.mousePosition, _lastTextureDrawRect, _texture.width, _texture.height);
            Vector2Int delta = mouseTex - _dragStartMouseTex;

            RectInt updated = _dragStartCropRect;
            switch (_dragMode)
            {
                case DragMode.Move:
                    updated.x += delta.x;
                    updated.y += delta.y;
                    break;
                case DragMode.Left:
                    updated.xMin += delta.x;
                    break;
                case DragMode.Right:
                    updated.xMax += delta.x;
                    break;
                case DragMode.Top:
                    updated.yMax += delta.y;
                    break;
                case DragMode.Bottom:
                    updated.yMin += delta.y;
                    break;
                case DragMode.TopLeft:
                    updated.xMin += delta.x;
                    updated.yMax += delta.y;
                    break;
                case DragMode.TopRight:
                    updated.xMax += delta.x;
                    updated.yMax += delta.y;
                    break;
                case DragMode.BottomLeft:
                    updated.xMin += delta.x;
                    updated.yMin += delta.y;
                    break;
                case DragMode.BottomRight:
                    updated.xMax += delta.x;
                    updated.yMin += delta.y;
                    break;
            }

            _cropRect = ClampRectToTexture(EnforceMinSize(updated, _dragStartCropRect, _dragMode), _texture);
            Repaint();
            e.Use();
            return;
        }

        if (e.type == EventType.MouseUp && e.button == 0 && _dragMode != DragMode.None)
        {
            _dragMode = DragMode.None;
            GUIUtility.hotControl = 0;
            e.Use();
        }

        UpdateCursors(selectionGuiRect);
    }

    private void UpdateCursors(Rect selectionGuiRect)
    {
        EditorGUIUtility.AddCursorRect(GetHandleRect(selectionGuiRect, DragMode.TopLeft), MouseCursor.ResizeUpLeft);
        EditorGUIUtility.AddCursorRect(GetHandleRect(selectionGuiRect, DragMode.BottomRight), MouseCursor.ResizeUpLeft);
        EditorGUIUtility.AddCursorRect(GetHandleRect(selectionGuiRect, DragMode.TopRight), MouseCursor.ResizeUpRight);
        EditorGUIUtility.AddCursorRect(GetHandleRect(selectionGuiRect, DragMode.BottomLeft), MouseCursor.ResizeUpRight);
        EditorGUIUtility.AddCursorRect(GetHandleRect(selectionGuiRect, DragMode.Left), MouseCursor.ResizeHorizontal);
        EditorGUIUtility.AddCursorRect(GetHandleRect(selectionGuiRect, DragMode.Right), MouseCursor.ResizeHorizontal);
        EditorGUIUtility.AddCursorRect(GetHandleRect(selectionGuiRect, DragMode.Top), MouseCursor.ResizeVertical);
        EditorGUIUtility.AddCursorRect(GetHandleRect(selectionGuiRect, DragMode.Bottom), MouseCursor.ResizeVertical);

        if (selectionGuiRect.Contains(Event.current.mousePosition))
        {
            DragMode hit = HitTest(selectionGuiRect, Event.current.mousePosition);
            if (hit == DragMode.Move)
            {
                EditorGUIUtility.AddCursorRect(selectionGuiRect, MouseCursor.MoveArrow);
            }
        }
    }

    private DragMode HitTest(Rect selectionGuiRect, Vector2 mousePos)
    {
        DragMode[] priority =
        {
            DragMode.TopLeft,
            DragMode.TopRight,
            DragMode.BottomLeft,
            DragMode.BottomRight,
            DragMode.Left,
            DragMode.Right,
            DragMode.Top,
            DragMode.Bottom
        };

        foreach (DragMode mode in priority)
        {
            if (GetHandleRect(selectionGuiRect, mode).Contains(mousePos))
            {
                return mode;
            }
        }

        if (selectionGuiRect.Contains(mousePos))
        {
            return DragMode.Move;
        }

        return DragMode.None;
    }

    private static Rect GetHandleRect(Rect selectionGuiRect, DragMode mode)
    {
        Vector2 center = mode switch
        {
            DragMode.TopLeft => new Vector2(selectionGuiRect.xMin, selectionGuiRect.yMin),
            DragMode.TopRight => new Vector2(selectionGuiRect.xMax, selectionGuiRect.yMin),
            DragMode.BottomLeft => new Vector2(selectionGuiRect.xMin, selectionGuiRect.yMax),
            DragMode.BottomRight => new Vector2(selectionGuiRect.xMax, selectionGuiRect.yMax),
            DragMode.Left => new Vector2(selectionGuiRect.xMin, selectionGuiRect.center.y),
            DragMode.Right => new Vector2(selectionGuiRect.xMax, selectionGuiRect.center.y),
            DragMode.Top => new Vector2(selectionGuiRect.center.x, selectionGuiRect.yMin),
            DragMode.Bottom => new Vector2(selectionGuiRect.center.x, selectionGuiRect.yMax),
            _ => selectionGuiRect.center
        };

        return new Rect(center.x - HandleSize * 0.5f, center.y - HandleSize * 0.5f, HandleSize, HandleSize);
    }

    private static Rect GetTextureDrawRect(Rect containerRect, int texWidth, int texHeight)
    {
        if (texWidth <= 0 || texHeight <= 0)
        {
            return containerRect;
        }

        float texAspect = (float)texWidth / texHeight;
        float containerAspect = containerRect.width / containerRect.height;

        if (containerAspect > texAspect)
        {
            float height = containerRect.height;
            float width = height * texAspect;
            float x = containerRect.x + (containerRect.width - width) * 0.5f;
            return new Rect(x, containerRect.y, width, height);
        }
        else
        {
            float width = containerRect.width;
            float height = width / texAspect;
            float y = containerRect.y + (containerRect.height - height) * 0.5f;
            return new Rect(containerRect.x, y, width, height);
        }
    }

    private static Rect TextureRectToGuiRect(RectInt texRect, Rect texDrawRect, int texWidth, int texHeight)
    {
        float xMin = texDrawRect.x + texRect.xMin / (float)texWidth * texDrawRect.width;
        float xMax = texDrawRect.x + texRect.xMax / (float)texWidth * texDrawRect.width;

        float yMaxFromTop = (texHeight - texRect.yMin) / (float)texHeight * texDrawRect.height;
        float yMinFromTop = (texHeight - texRect.yMax) / (float)texHeight * texDrawRect.height;

        float yMin = texDrawRect.y + yMinFromTop;
        float yMax = texDrawRect.y + yMaxFromTop;

        return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    }

    private static Vector2Int GuiToTexturePointInt(Vector2 guiPos, Rect texDrawRect, int texWidth, int texHeight)
    {
        float u = Mathf.InverseLerp(texDrawRect.xMin, texDrawRect.xMax, guiPos.x);
        float v = Mathf.InverseLerp(texDrawRect.yMin, texDrawRect.yMax, guiPos.y);

        int x = Mathf.RoundToInt(Mathf.Lerp(0, texWidth, u));
        int y = Mathf.RoundToInt(Mathf.Lerp(texHeight, 0, v));

        x = Mathf.Clamp(x, 0, texWidth);
        y = Mathf.Clamp(y, 0, texHeight);
        return new Vector2Int(x, y);
    }

    private static (int cellWidth, int cellHeight) ComputeCellSize(RectInt cropRect, int rows, int cols, int frameSpacing)
    {
        if (rows <= 0 || cols <= 0)
        {
            return (0, 0);
        }

        int usableW = cropRect.width - (cols - 1) * frameSpacing;
        int usableH = cropRect.height - (rows - 1) * frameSpacing;
        return (usableW / cols, usableH / rows);
    }

    private static bool IsValidForSlicing(Texture2D texture, RectInt cropRect, int rows, int cols, int frameSpacing, int paddingLeft, int paddingRight, int paddingTop, int paddingBottom)
    {
        if (texture == null)
        {
            return false;
        }

        cropRect = ClampRectToTexture(cropRect, texture);
        if (cropRect.width < 1 || cropRect.height < 1)
        {
            return false;
        }

        if (rows < 1 || cols < 1 || frameSpacing < 0)
        {
            return false;
        }

        if (paddingLeft < 0 || paddingRight < 0 || paddingTop < 0 || paddingBottom < 0)
        {
            return false;
        }

        (int cellW, int cellH) = ComputeCellSize(cropRect, rows, cols, frameSpacing);
        if (cellW <= paddingLeft + paddingRight || cellH <= paddingTop + paddingBottom)
        {
            return false;
        }

        return true;
    }

    private static RectInt ClampRectToTexture(RectInt rect, Texture2D texture)
    {
        if (texture == null)
        {
            return rect;
        }

        int xMin = Mathf.Clamp(rect.xMin, 0, texture.width);
        int xMax = Mathf.Clamp(rect.xMax, 0, texture.width);
        int yMin = Mathf.Clamp(rect.yMin, 0, texture.height);
        int yMax = Mathf.Clamp(rect.yMax, 0, texture.height);

        if (xMax < xMin)
        {
            (xMax, xMin) = (xMin, xMax);
        }

        if (yMax < yMin)
        {
            (yMax, yMin) = (yMin, yMax);
        }

        RectInt clamped = new RectInt(xMin, yMin, xMax - xMin, yMax - yMin);
        if (clamped.width < 1)
        {
            clamped.width = 1;
        }

        if (clamped.height < 1)
        {
            clamped.height = 1;
        }

        if (clamped.xMax > texture.width)
        {
            clamped.x = Mathf.Max(0, texture.width - clamped.width);
        }

        if (clamped.yMax > texture.height)
        {
            clamped.y = Mathf.Max(0, texture.height - clamped.height);
        }

        return clamped;
    }

    private static RectInt EnforceMinSize(RectInt updated, RectInt original, DragMode mode)
    {
        if (updated.width >= MinSelectionSize && updated.height >= MinSelectionSize)
        {
            return updated;
        }

        RectInt rect = updated;

        if (rect.width < MinSelectionSize)
        {
            int deficit = MinSelectionSize - rect.width;
            if (mode is DragMode.Left or DragMode.TopLeft or DragMode.BottomLeft)
            {
                rect.xMin -= deficit;
            }
            else
            {
                rect.xMax += deficit;
            }
        }

        if (rect.height < MinSelectionSize)
        {
            int deficit = MinSelectionSize - rect.height;
            if (mode is DragMode.Bottom or DragMode.BottomLeft or DragMode.BottomRight)
            {
                rect.yMin -= deficit;
            }
            else
            {
                rect.yMax += deficit;
            }
        }

        if (rect.width < 1 || rect.height < 1)
        {
            return original;
        }

        return rect;
    }
}
