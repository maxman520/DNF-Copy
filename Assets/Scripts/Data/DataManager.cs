using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class DataManager : Singleton<DataManager>
{

    private AccountData accountData;
    private string dataPath; // 데이터가 저장될 파일의 전체 경로

    public CharacterData SelectedCharacter { get; private set; }

    protected override void Awake()
    {
        base.Awake(); // 싱글톤 설정
        dataPath = Path.Combine(Application.persistentDataPath, "CharacterData.json"); // 데이터 경로 설정
        LoadData(); // 저장된 데이터가 있는지 확인하고 불러옴
    }

    public void LoadData()
    {
        // 파일이 있을 경우
        if (File.Exists(dataPath))
        {
            string json = File.ReadAllText(dataPath);
            accountData = JsonUtility.FromJson<AccountData>(json);
            Debug.Log($"데이터 로드 완료. 캐릭터 수: {accountData.Characters.Count}");
        }
        // 파일이 없을 경우 (게임을 처음 실행한 경우)
        else
        {
            accountData = new AccountData();
            Debug.Log("저장된 데이터 없음. 새로운 AccountData 생성.");
        }
    }

    // 데이터를 저장하는 함수
    private void SaveData()
    {
        string json = JsonUtility.ToJson(accountData, true); // accountData의 캐릭터 데이터를 JSON 형식의 문자열로 변환
        File.WriteAllText(dataPath, json); // 변환된 JSON 문자열을 dataPath에 파일로 기록. 이미 존재하면 덮어씀
        Debug.Log($"데이터 저장 완료. 경로: {dataPath}");
    }

    // 새로운 캐릭터 추가 함수
    public void AddCharacter(CharacterData newCharacter)
    {
        accountData.Characters.Add(newCharacter); // 새로운 캐릭터 정보를 받아서 accountData의 리스트에 추가
        SaveData(); // 변경 사항을 파일에 바로 저장
        Debug.Log($"캐릭터 추가됨: {newCharacter.CharacterName} ({newCharacter.JobName}). 총 캐릭터 수: {accountData.Characters.Count}");
    }

    // 캐릭터 선택 시 호출되어 선택된 캐릭터 정보를 저장
    public void SelectCharacter(CharacterData data)
    {
        SelectedCharacter = data;
        Debug.Log($"캐릭터 선택: {data.CharacterName}");
    }

    // 현재까지 생성된 모든 캐릭터의 목록을 요청한 스크립트에게 전달
    public List<CharacterData> GetCharacters()
    {
        return accountData.Characters;
    }

    // 캐릭터 삭제 함수
    public void DeleteCharacter(CharacterData characterToDelete)
    {
        if (accountData.Characters.Remove(characterToDelete))
        {
            SaveData(); // 변경 사항을 파일에 바로 저장
            Debug.Log($"캐릭터 삭제됨: {characterToDelete.CharacterName}. 총 캐릭터 수: {accountData.Characters.Count}");
        }
        else
        {
            Debug.LogWarning($"삭제하려는 캐릭터를 찾지 못했습니다: {characterToDelete.CharacterName}");
        }
    }

    // 현재 선택된 캐릭터의 데이터를 업데이트하고 저장하는 함수
    public void UpdateAndSaveCurrentCharacter(CharacterData updatedData)
    {
        if (SelectedCharacter == null)
        {
            Debug.LogWarning("선택된 캐릭터가 없어 저장할 수 없습니다.");
            return;
        }

        // 계정 데이터에서 현재 선택된 캐릭터와 동일한 닉네임을 가진 캐릭터를 찾음
        int index = accountData.Characters.FindIndex(c => c.CharacterName == SelectedCharacter.CharacterName);

        if (index != -1)
        {
            // 찾았으면 데이터 업데이트
            accountData.Characters[index] = updatedData;
            SelectedCharacter = updatedData; // 현재 선택된 캐릭터 정보도 최신화
            SaveData();
            Debug.Log($"캐릭터 데이터 업데이트 및 저장 완료: {updatedData.CharacterName}");
        }
        else
        {
            Debug.LogWarning($"저장할 캐릭터를 계정 목록에서 찾지 못했습니다: {SelectedCharacter.CharacterName}");
        }
    }
}
