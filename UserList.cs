using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorBricks
{
	public class UserList : IEnumerable<User>
	{
		private Dictionary<int, User> dict = new Dictionary<int, User>();

		public int Count
		{
			get
			{
				return dict.Count;
			}
		}

		public User this[int id]
		{
			get
			{
				return dict[id];
			}
		}

        /// <summary>
        /// Returns the number of users (including corrupt ones) who have no unblocked bridges.
        /// </summary>
		public IEnumerable<User> ThirstyUsers
		{
			get
			{
				return dict.Values.Where(u => u.IsThirsty);
			}
		}

		public int[] GetIds()
		{
			return dict.Keys.ToArray();
		}

		public User[] GetUsers()
		{
            return dict.Values.ToArray();
		}

		public void Add(User u)
		{
			if (dict.ContainsKey(u.Id))
				throw new InvalidOperationException("User already exists in the list!");

			dict[u.Id] = u;
		}

		public void Remove(User u)
		{
			if (!dict.ContainsKey(u.Id))
				throw new InvalidOperationException("User does not exist in the list!");

			dict.Remove(u.Id);
		}
	
		public IEnumerator<User> GetEnumerator()
		{
			return dict.Values.GetEnumerator();
		}

        IEnumerator IEnumerable.GetEnumerator()
        {
            return dict.Values.GetEnumerator();
        }
	}
}
