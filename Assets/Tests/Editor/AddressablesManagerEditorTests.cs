using NUnit.Framework;
using UnityEngine;
using UnityEditor; // Required for cleaning up GameObjects in editor tests

public class AddressablesManagerEditorTests
{
    private GameObject _managerGameObject;

    [TearDown]
    public void Teardown()
    {
        // Clean up the GameObject created by the singleton instance
        if (_managerGameObject != null)
        {
            Object.DestroyImmediate(_managerGameObject);
            _managerGameObject = null;
        }
        // Reset the singleton instance if possible (requires a static setter or a reset method in AddressablesManager)
        // For this example, we rely on the getter creating a new one if null,
        // but ideally, AddressablesManager would have a static method like AddressablesManager.ResetInstanceForTests().
        // If _instance is private and has no such method, this teardown might not fully reset state for subsequent tests in a batch.
        // However, NUnit typically runs tests in separate contexts or reloads assemblies.

        // Accessing private static field for reset (if absolutely necessary and understood to be fragile)
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
        // Act
        var instance = AddressablesManager.Instance;
        _managerGameObject = instance.gameObject; // Store for cleanup

        // Assert
        Assert.IsNotNull(instance, "AddressablesManager.Instance should not be null.");
    }

    [Test]
    public void TestSingletonInstance_ReturnsSameInstance()
    {
        // Act
        var instance1 = AddressablesManager.Instance;
        _managerGameObject = instance1.gameObject; // Store for cleanup (will be the same object)
        var instance2 = AddressablesManager.Instance;

        // Assert
        Assert.AreSame(instance1, instance2, "AddressablesManager.Instance should return the same instance on multiple accesses.");
    }

    [Test]
    public void TestSingletonInstance_CreatesGameObject()
    {
        // Act
        var instance = AddressablesManager.Instance;
        _managerGameObject = instance.gameObject;

        // Assert
        Assert.IsNotNull(_managerGameObject, "AddressablesManager singleton should create its own GameObject.");
        Assert.AreEqual("AddressablesManager", _managerGameObject.name, "GameObject name should be 'AddressablesManager'.");
        Assert.IsTrue(_managerGameObject.GetComponent<AddressablesManager>() == instance, "GameObject should have the AddressablesManager component.");
    }

    // Note: Further editor tests for AddressablesManager might be limited
    // as many of its functionalities (loading, updates) are runtime features
    // and depend on the Addressables system being initialized, which is typically a runtime process.
    // Testing methods like CheckForCatalogUpdates in editor mode without Addressables initialized
    // would likely not be meaningful or might throw errors.
}
