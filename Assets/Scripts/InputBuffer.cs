using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Linq 사용을 위해 추가

public class InputBuffer
{
    private readonly Queue<(ICommand command, float timestamp)> buffer = new Queue<(ICommand, float)>();
    private readonly float bufferTime; // 버퍼에 들어온 입력이 유효한 시간 (초)

    public InputBuffer(float bufferTime)
    {
        this.bufferTime = bufferTime;
    }

    public void AddCommand(ICommand command)
    {
        // (커맨드, 생성 시간) 튜플을 큐에 추가
        buffer.Enqueue((command, Time.time));
    }

    // 버퍼의 맨 앞 커맨드를 '들여다보기'
    public ICommand PeekCommand()
    {
        // 너무 오래된 커맨드는 여기서 정리
        while (buffer.Count > 0 && Time.time - buffer.Peek().timestamp > bufferTime)
        {
            buffer.Dequeue();
        }

        if (buffer.Count > 0)
        {
            // 큐의 맨 앞 요소를 반환
            return buffer.Peek().command;
        }

        return null;
    }

    // 버퍼의 맨 앞 커맨드를 제거
    public void RemoveCommand()
    {
        if (buffer.Count > 0)
        {
            buffer.Dequeue();
        }
    }

    // 디버깅을 위해 현재 버퍼의 내용을 문자열 리스트로 반환하는 함수
    public List<string> GetBufferedCommandNames()
    {
        // 현재 버퍼의 내용을 복사하여 처리 (원본 훼손 방지)
        return buffer.Select(item => $"{item.command.GetType().Name} ({(bufferTime - (Time.time - item.timestamp)):F2}s left)").ToList();
    }
}