﻿namespace BCad
{
    public interface IUndoRedoService
    {
        void SetSnapshot();
        void Undo();
        void Redo();
        void ClearHistory();
        int UndoHistorySize { get; }
        int RedoHistorySize { get; }
    }
}