using NUnit.Framework;
using UnityEngine;
using UnityEditor; // 에디터 테스트에서 GameObjects를 정리하기 위해 필요

public class AddressablesManagerEditorTests
{
    private GameObject _managerGameObject;

    [TearDown]
    public void Teardown()
    {
        // 싱글톤 인스턴스에 의해 생성된 GameObject 정리
        if (_managerGameObject != null)
        {
            Object.DestroyImmediate(_managerGameObject);
            _managerGameObject = null;
        }
        // 가능한 경우 싱글톤 인스턴스 초기화 (AddressablesManager에 static setter나 reset 메서드 필요)
        // 이 예제에서는 getter가 null일 때 새로운 인스턴스를 생성하는 것에 의존하지만,
        // 이상적으로는 AddressablesManager에 AddressablesManager.ResetInstanceForTests()와 같은 static 메서드가 있어야 합니다.
        // _instance가 private이고 그런 메서드가 없다면, 이 teardown은 배치의 후속 테스트를 위해 상태를 완전히 초기화하지 못할 수 있습니다.
        // 하지만 NUnit은 일반적으로 테스트를 별도의 컨텍스트에서 실행하거나 어셈블리를 다시 로드합니다.

        // 초기화를 위해 private static 필드에 접근 (절대적으로 필요하고 취약하다고 이해되는 경우)
        var type = typeof(AddressablesManager);
        var instanceField = type.GetField("_instance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        if (instanceField != null)
        {
            instanceField.SetValue(null, null);
        }
    }

    [Test]
    public void TestSingletonInstance_IsNotNull()
    {
        // 실행
        var instance = AddressablesManager.Instance;
        _managerGameObject = instance.gameObject; // 정리를 위해 저장

        // 검증
        Assert.IsNotNull(instance, "AddressablesManager.Instance는 null이 아니어야 합니다.");
    }

    [Test]
    public void TestSingletonInstance_ReturnsSameInstance()
    {
        // 실행
        var instance1 = AddressablesManager.Instance;
        _managerGameObject = instance1.gameObject; // 정리를 위해 저장 (동일한 객체)
        var instance2 = AddressablesManager.Instance;

        // 검증
        Assert.AreSame(instance1, instance2, "AddressablesManager.Instance는 여러 번 접근해도 동일한 인스턴스를 반환해야 합니다.");
    }

    [Test]
    public void TestSingletonInstance_CreatesGameObject()
    {
        // 실행
        var instance = AddressablesManager.Instance;
        _managerGameObject = instance.gameObject;

        // 검증
        Assert.IsNotNull(_managerGameObject, "AddressablesManager 싱글톤은 자체 GameObject를 생성해야 합니다.");
        Assert.AreEqual("AddressablesManager", _managerGameObject.name, "GameObject 이름은 'AddressablesManager'여야 합니다.");
        Assert.IsTrue(_managerGameObject.GetComponent<AddressablesManager>() == instance, "GameObject는 AddressablesManager 컴포넌트를 가져야 합니다.");
    }

    // 참고: AddressablesManager에 대한 추가 에디터 테스트는 제한적일 수 있습니다.
    // 많은 기능들(로딩, 업데이트)은 런타임 기능이며
    // Addressables 시스템이 초기화되어 있어야 하는데, 이는 일반적으로 런타임 프로세스입니다.
    // Addressables가 초기화되지 않은 에디터 모드에서 CheckForCatalogUpdates와 같은 메서드를 테스트하는 것은
    // 의미가 없거나 오류를 발생시킬 수 있습니다.
}
