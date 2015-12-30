using System.Collections.Generic;
using System.Dynamic;
using System;
using System.Runtime.CompilerServices;

namespace dks.Templating
{
	class RazorDynamicObject : DynamicObject
	{
		#region Properties
		/// <summary>
		/// Gets or sets the model.
		/// </summary>
		public object Model { get; set; }
		#endregion

		#region Methods
		/// <summary>
		/// Gets the value of the specified member.
		/// </summary>
		/// <param name="binder">The current binder.</param>
		/// <param name="result">The member result.</param>
		/// <returns>True.</returns>
		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			var dynamicObject = Model as RazorDynamicObject;
			if (dynamicObject != null)
				return dynamicObject.TryGetMember(binder, out result);

			Type modelType = Model.GetType();

			var prop = modelType.GetProperty(binder.Name);
			var field = modelType.GetField(binder.Name);

			if (prop == null && field == null)
			{
				result = null;
				return false;
			}

			object value = prop != null ? prop.GetValue(Model, null) : field.GetValue(Model);
			if (value == null)
			{
				result = null;
				return true;
			}

			Type valueType = value.GetType();
			//result = (IsAnonymousType(valueType)) ? new RazorDynamicObject { Model = value } : value;

			result = valueType.FullName.StartsWith("System.") ? value : new RazorDynamicObject { Model = value };

			return true;
		}

		//private static bool IsAnonymousType(Type type)
		//{
		//	if (type == null)
		//		throw new ArgumentNullException("type");

		//	return (type.IsClass
		//			&& type.IsSealed
		//			&& type.BaseType == typeof(object)
		//			&& (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$Anonymous"))
		//			&& type.IsDefined(typeof(CompilerGeneratedAttribute), true));
		//}

		#endregion
	}
}