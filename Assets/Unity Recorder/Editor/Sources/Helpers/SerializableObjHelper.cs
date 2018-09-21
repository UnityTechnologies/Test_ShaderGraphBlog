using System;
using System.Linq.Expressions;

namespace UnityEditor.Recorder
{
    static class SerializableObjHelper
    {       
        public static SerializedProperty FindPropertyRelative(this SerializedProperty obj, Expression<Func<object>> exp)
        {
            var body = exp.Body as MemberExpression;
            if (body == null)
            {
                var ubody = (UnaryExpression)exp.Body;
                body = ubody.Operand as MemberExpression;
            }

            var name = body.Member.Name;

            return obj.FindPropertyRelative(name);
        }
    }

    class PropertyFinder<TType> where TType : class
    {
        SerializedObject m_Obj;
        public PropertyFinder(SerializedObject obj)
        {
            m_Obj = obj;
        }

        public delegate TResult FuncX<TResult>(TType x);
        public SerializedProperty Find( Expression<FuncX<object>> exp)
        {
            var body = exp.Body as MemberExpression;
            if (body == null)
            {
                var ubody = (UnaryExpression)exp.Body;
                body = ubody.Operand as MemberExpression;
            }

            var name = body.Member.Name;

            return m_Obj.FindProperty(name);
        }

    }
}
