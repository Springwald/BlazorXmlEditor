using System;

namespace de.springwald.xml.dtd
{
	/// <summary>
	/// Ein einzelnes DTD-Element aus einer DTD
	/// </summary>
	/// <remarks>
	/// (C)2005 Daniel Springwald, Herne Germany
	/// Springwald Software  - www.springwald.de
	/// daniel@springwald.de -   0700-SPRINGWALD
	/// all rights reserved
	/// </remarks>
	public class DTDEntity
	{

		#region PRIVATE ATTRIBUTES

		private string _name;			 // Der eindeutige Name dieser Entity
		private string _inhalt;			 // Der Inhalt dieser Entity
		private bool _istErsetzungsEntity;	// (% - Entity) - enthält nur einen zu ersetzenden String und bleibt nicht unter ihrem Namen als einzufügen bestehen
		
		#endregion

		#region PUBLIC ATTRIBUTES

		/// <summary>
		/// Der eindeutige Name  dieser Entity
		/// </summary>
		public string Name 
		{
			get { return _name; }
			set { _name = value; }
		}

		/// <summary>
		/// Der Inhalt dieser Entity
		/// </summary>
		public string Inhalt 
		{
			get { return _inhalt; }
			set { _inhalt = value; }
		}

		/// <summary>
		/// Ist eine eine % - Entity, d.h.enthält nur einen zu ersetzenden String und bleibt nicht unter ihrem Namen als einzufügen bestehen?
		/// </summary>
		public bool IstErsetzungsEntity 
		{
			get { return this._istErsetzungsEntity; }
			set { this._istErsetzungsEntity = value; }
		}

		#endregion

		#region PUBLIC METHODS

		/// <summary>
		/// Erzeugt eine DTD-Entity auf Basis des übergebenen DTD-Entity-Quellcodes
		/// </summary>
		public DTDEntity()
		{
		}

		#endregion

		#region PRIVATE METHODS

		#endregion
		
	}
}
