using System.Collections.Generic;
public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ AOT assemblies
	public static readonly IReadOnlyList<string> PatchedAOTAssemblyList = new List<string>
	{
		"Newtonsoft.Json.dll",
		"UnityEngine.CoreModule.dll",
		"mscorlib.dll",
	};
	// }}

	// {{ constraint implement type
	// }} 

	// {{ AOT generic types
	// System.Action<Entry.MyVec3>
	// System.Action<object,object>
	// System.Action<object>
	// System.Collections.Generic.ArraySortHelper<Entry.MyVec3>
	// System.Collections.Generic.Comparer<Entry.MyVec3>
	// System.Collections.Generic.Comparer<byte>
	// System.Collections.Generic.Comparer<object>
	// System.Collections.Generic.EqualityComparer<Entry.MyVec3>
	// System.Collections.Generic.EqualityComparer<byte>
	// System.Collections.Generic.EqualityComparer<object>
	// System.Collections.Generic.ICollection<Entry.MyVec3>
	// System.Collections.Generic.IComparer<Entry.MyVec3>
	// System.Collections.Generic.IEnumerable<Entry.MyVec3>
	// System.Collections.Generic.IEnumerator<Entry.MyVec3>
	// System.Collections.Generic.IList<Entry.MyVec3>
	// System.Collections.Generic.List.Enumerator<Entry.MyVec3>
	// System.Collections.Generic.List.SynchronizedList<Entry.MyVec3>
	// System.Collections.Generic.List<Entry.MyVec3>
	// System.Collections.Generic.ObjectComparer<Entry.MyVec3>
	// System.Collections.Generic.ObjectComparer<byte>
	// System.Collections.Generic.ObjectComparer<object>
	// System.Collections.Generic.ObjectEqualityComparer<Entry.MyVec3>
	// System.Collections.Generic.ObjectEqualityComparer<byte>
	// System.Collections.Generic.ObjectEqualityComparer<object>
	// System.Collections.ObjectModel.ReadOnlyCollection<Entry.MyVec3>
	// System.Comparison<Entry.MyVec3>
	// System.Func<System.ValueTuple<object,byte>>
	// System.Func<object,System.ValueTuple<object,byte>>
	// System.Func<object,object,object>
	// System.Func<object,object>
	// System.Func<object>
	// System.Predicate<Entry.MyVec3>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<object,byte>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<object>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<object,byte>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<object>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<object,byte>>
	// System.Runtime.CompilerServices.TaskAwaiter<object>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<object,byte>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<object>
	// System.Threading.Tasks.Task.<>c<System.ValueTuple<object,byte>>
	// System.Threading.Tasks.Task.<>c<object>
	// System.Threading.Tasks.Task<System.ValueTuple<object,byte>>
	// System.Threading.Tasks.Task<object>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<object,byte>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<object>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_1<System.ValueTuple<object,byte>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_1<object>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<object,byte>>
	// System.Threading.Tasks.TaskFactory<object>
	// System.ValueTuple<object,byte>
	// }}

	public void RefMethods()
	{
		// object Newtonsoft.Json.JsonConvert.DeserializeObject<object>(string)
		// object Newtonsoft.Json.JsonConvert.DeserializeObject<object>(string,Newtonsoft.Json.JsonSerializerSettings)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<object,byte>>,TestShell.<RunHttpListener>d__3>(System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<object,byte>>&,TestShell.<RunHttpListener>d__3&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,TestShell.<RunHttpListener>d__3>(System.Runtime.CompilerServices.TaskAwaiter<object>&,TestShell.<RunHttpListener>d__3&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<TestShell.<RunHttpListener>d__3>(TestShell.<RunHttpListener>d__3&)
		// object UnityEngine.GameObject.AddComponent<object>()
	}
}