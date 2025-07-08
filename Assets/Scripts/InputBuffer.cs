using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Linq ����� ���� �߰�

public class InputBuffer
{
    private readonly Queue<(ICommand command, float timestamp)> buffer = new Queue<(ICommand, float)>();
    private readonly float bufferTime; // ���ۿ� ���� �Է��� ��ȿ�� �ð� (��)

    public InputBuffer(float bufferTime)
    {
        this.bufferTime = bufferTime;
    }

    public void AddCommand(ICommand command)
    {
        // (Ŀ�ǵ�, ���� �ð�) Ʃ���� ť�� �߰�
        buffer.Enqueue((command, Time.time));
    }

    // ������ �� �� Ŀ�ǵ带 '�鿩�ٺ���'
    public ICommand PeekCommand()
    {
        // �ʹ� ������ Ŀ�ǵ�� ���⼭ ����
        while (buffer.Count > 0 && Time.time - buffer.Peek().timestamp > bufferTime)
        {
            buffer.Dequeue();
        }

        if (buffer.Count > 0)
        {
            // ť�� �� �� ��Ҹ� ��ȯ
            return buffer.Peek().command;
        }

        return null;
    }

    // ������ �� �� Ŀ�ǵ带 ����
    public void RemoveCommand()
    {
        if (buffer.Count > 0)
        {
            buffer.Dequeue();
        }
    }

    // ������� ���� ���� ������ ������ ���ڿ� ����Ʈ�� ��ȯ�ϴ� �Լ�
    public List<string> GetBufferedCommandNames()
    {
        // ���� ������ ������ �����Ͽ� ó�� (���� �Ѽ� ����)
        return buffer.Select(item => $"{item.command.GetType().Name} ({(bufferTime - (Time.time - item.timestamp)):F2}s left)").ToList();
    }
}