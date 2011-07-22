﻿using System;
using Ponykart.Core;

namespace Ponykart.Levels
{
	public delegate void LevelEventHandler(LevelChangedEventArgs eventArgs);

	public class LevelManager
	{
		public Level CurrentLevel { get; private set; }
		public event LevelEventHandler OnLevelLoad;
		public event LevelEventHandler OnLevelUnload;

		/// <summary>
		/// constructor
		/// </summary>
		public LevelManager()
		{
			this.IsValidLevel = false;
		}

		private bool hasRunPostInitEvents = false;
		/// <summary>
		/// runs level manager stuff that needs to run immediately after kernel setup
		/// </summary>
		public void RunPostInitEvents() {
			// don't let this run twice
			if (hasRunPostInitEvents)
				throw new ApplicationException("The LevelManager has already run its post-initialisation events!");

			CurrentLevel = new Level(Settings.Default.MainMenuName);

			// run level loading events
			if (OnLevelLoad != null)
				OnLevelLoad(new LevelChangedEventArgs(CurrentLevel, new Level(null)));

			IsValidLevel = true;

			// make sure this won't run again
			hasRunPostInitEvents = true;
			// pause it for the main menu
			Pauser.IsPaused = true;
			// we don't want any input to go while we're in the middle of changing levels
			LKernel.Get<InputSwallowerManager>().AddSwallower(() => !IsValidLevel, this);
		}

		/// <summary>
		/// Unloads the current level
		/// - Sets IsValidLevel to false
		/// - Runs the levelunload events
		/// - Tells the kernel to unload all level objects
		/// </summary>
		private void UnloadLevel(LevelChangedEventArgs eventArgs)
		{
			if (CurrentLevel.Name != null)
			{
				Launch.Log("======= Level unloading: " + CurrentLevel.Name + " =======");

				IsValidLevel = false;

				CurrentLevel.Save();
				
				// invoke the level unloading events
				if (OnLevelUnload != null)
					OnLevelUnload(eventArgs);

				LKernel.UnloadLevelObjects();		
			}
		}

		/// <summary>
		/// Unloads the current level and loads the new level
		/// </summary>
		/// <param name="newLevelName">The name of the level to load</param>
		public void LoadLevel(string newLevelName)
		{
			Pauser.IsPaused = false;
			Level oldLevel = CurrentLevel;
			Level newLevel = new Level(newLevelName);
			var eventArgs = new LevelChangedEventArgs(newLevel, oldLevel);

			// Unload current level
			UnloadLevel(eventArgs);

			CurrentLevel = newLevel;
			
			// Load new Level
			if (newLevel != null)
			{
				Launch.Log("======= Level loading: " + newLevel.Name + " =======");

				// bind level stuff to the kernel
				LKernel.LoadLevelObjects(eventArgs);
				// create the enviroment
				newLevel.CreateEnvironment();

				// run our level loading events
				Launch.Log("[Loading] Loading everything else...");
				if (OnLevelLoad != null)
					OnLevelLoad(eventArgs);

				// then put Things into our world
				newLevel.CreateEntities();
				// then load the rest of the handlers
				LKernel.CreateCamera();
				LKernel.LoadLevelHandlers();

				IsValidLevel = true;

				// run our scripts
				newLevel.RunLevelScripts();
			}

			// if we're on the main menu, pause it
			if (newLevel.Name == Settings.Default.MainMenuName)
				Pauser.IsPaused = true;

			// get rid of the old level and clean up
			oldLevel.Dispose();
			GC.Collect();
		}

		/// <summary>
		/// Tells whether this level is valid or not.
		/// </summary>
		public bool IsValidLevel { get; private set; }

		/// <summary>
		/// Returns true if the current level is valid and not a main menu
		/// </summary>
		public bool IsPlayableLevel {
			get {
				return CurrentLevel != null && CurrentLevel.Name != Settings.Default.MainMenuName;
			}
		}
	}

}