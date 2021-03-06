#region License
/*
Microsoft Public License (Ms-PL)
XnaTouch - Copyright © 2009 The XnaTouch Team

All rights reserved.

This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
accept the license, do not use the software.

1. Definitions
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the software.
A "contributor" is any person that distributes its contribution under this license.
"Licensed patents" are a contributor's patent claims that read directly on its contribution.

2. Grant of Rights
(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

3. Conditions and Limitations
(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
your patent license from such contributor to the software ends automatically.
(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
notices that are present in the software.
(D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
code form, you may only do so under a license that complies with this license.
(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
purpose and non-infringement.
*/
#endregion License

#region Using Statements
using System;
using System.Drawing;
using System.Collections.Generic;

using MonoTouch.CoreAnimation;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.OpenGLES;
using MonoTouch.UIKit;

using OpenTK.Platform.iPhoneOS;

using OpenTK;
using OpenTK.Platform;
using OpenTK.Graphics;
using OpenTK.Graphics.ES11;
using OpenTK.Graphics.ES20;

using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
#endregion Using Statements

namespace Microsoft.Xna.Framework
{
    public class GameWindow : iPhoneOSGameView
    {
		private readonly Rectangle clientBounds;
		internal Game game;
		private GameTime _updateGameTime;
        private GameTime _drawGameTime;
        private DateTime _lastUpdate;
		private DateTime _now;
		
		UITapGestureRecognizer recognizerTap;
		UITapGestureRecognizer recognizerDoubleTap;
		UIPinchGestureRecognizer recognizerPinch; 
		UISwipeGestureRecognizer recognizerSwipe;
		UILongPressGestureRecognizer recognizerLongPress;
		UIPanGestureRecognizer recognizerPan;
		UIRotationGestureRecognizer recognizerRotation;		
		
		public EAGLContext MainContext;
	    public EAGLContext BackgroundContext;
	    public EAGLSharegroup ShareGroup; 
				
		#region UIVIew Methods

		public GameWindow() : base (UIScreen.MainScreen.Bounds)
		{
			LayerRetainsBacking = false; 
			LayerColorFormat	= EAGLColorFormat.RGBA8;
			ContentScaleFactor  = UIScreen.MainScreen.Scale;
			
			RectangleF rect = UIScreen.MainScreen.Bounds;
			clientBounds = new Rectangle(0,0,(int) (rect.Width * UIScreen.MainScreen.Scale),(int) (rect.Height * UIScreen.MainScreen.Scale));
			
			// Enable multi-touch
			MultipleTouchEnabled = true;	
						
			// Initialize GameTime
            _updateGameTime = new GameTime();
            _drawGameTime = new GameTime(); 
			
			// Initialize _lastUpdate
			_lastUpdate = DateTime.Now;
		}	
		
		~GameWindow()
		{
			//
		}
		
		[Export ("layerClass")]
		static Class LayerClass() 
		{
			return iPhoneOSGameView.GetLayerClass ();
		}
		
		protected override void ConfigureLayer(CAEAGLLayer eaglLayer) 
		{
			eaglLayer.Opaque = true;
		}
		
		protected override void CreateFrameBuffer()
		{	    
			try
			{
		        // TODO ContextRenderingApi = EAGLRenderingAPI.OpenGLES2;
				ContextRenderingApi = EAGLRenderingAPI.OpenGLES1;
				base.CreateFrameBuffer();
		    } 
			catch (Exception) 
			{
		        // device doesn't support OpenGLES 2.0; retry with 1.1:
		        ContextRenderingApi = EAGLRenderingAPI.OpenGLES1;
				base.CreateFrameBuffer();
		    }
			
			
		}
		
		#endregion
		
		#region iPhoneOSGameView Methods
		
		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);
		}
		
		protected override void OnDisposed(EventArgs e)
		{
			base.OnDisposed(e);
		}
		
		protected override void OnLoad (EventArgs e)
		{
			base.OnLoad(e);
			
			var enabledGestures = TouchPanel.EnabledGestures;
			if ( enabledGestures != GestureType.None )
			{
				if ((enabledGestures & GestureType.Hold) != 0)
				{
					recognizerLongPress = new UILongPressGestureRecognizer(this, new Selector ("LongPressGestureRecognizer"));		
					recognizerLongPress.MinimumPressDuration = 1.0;
					AddGestureRecognizer(recognizerLongPress);
				}
				
				if ((enabledGestures & GestureType.Tap) != 0)
				{
					recognizerTap = new UITapGestureRecognizer(this, new Selector ("TapGestureRecognizer"));
					recognizerTap.NumberOfTapsRequired = 1;
					AddGestureRecognizer(recognizerTap);
				}
				
				if ((enabledGestures & GestureType.DoubleTap) != 0)
				{
					recognizerDoubleTap = new UITapGestureRecognizer(this, new Selector ("TapGestureRecognizer"));
					recognizerDoubleTap.NumberOfTapsRequired = 2;
					AddGestureRecognizer(recognizerDoubleTap);
				}
			
				if ((enabledGestures & GestureType.FreeDrag) != 0)
				{
					recognizerPan = new UIPanGestureRecognizer(this, new Selector ("PanGestureRecognizer"));					
					AddGestureRecognizer(recognizerPan);
				}
				
				if ((enabledGestures & GestureType.Flick) != 0)
				{			
					recognizerSwipe = new UISwipeGestureRecognizer(this, new Selector ("SwipeGestureRecognizer"));		
					AddGestureRecognizer(recognizerSwipe);
				}
			
				if ((enabledGestures & GestureType.Pinch) != 0)
				{
					recognizerPinch = new UIPinchGestureRecognizer(this, new Selector ("PinchGestureRecognizer"));		
					AddGestureRecognizer(recognizerPinch);
				}
				
				if ((enabledGestures & GestureType.Rotation) != 0)
				{
					recognizerRotation = new UIRotationGestureRecognizer(this, new Selector ("RotationGestureRecognizer"));		
					AddGestureRecognizer(recognizerRotation);
				}	
				
				
			}
		}
		
		protected override void OnRenderFrame(FrameEventArgs e)
		{
			base.OnRenderFrame(e);
			
			MakeCurrent();
						
			// This code was commented to make the code base more iPhone like.
			// More speed testing is required, to see if this is worse or better
			// game.DoStep();	
			
			if (game != null )
			{
				_drawGameTime.Update(_now - _lastUpdate);
            	_lastUpdate = _now;
            	game.DoDraw(_drawGameTime);
			}
						
			SwapBuffers();
		}
		
		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
		}
		
		protected override void OnTitleChanged(EventArgs e)
		{
			base.OnTitleChanged(e);
		}
		
		protected override void OnUnload(EventArgs e)
		{
			base.OnUnload(e);
		}
		
		protected override void OnUpdateFrame(FrameEventArgs e)
		{			
			base.OnUpdateFrame(e);	
			
			if (game != null )
			{
				_now = DateTime.Now;
				_updateGameTime.Update(_now - _lastUpdate);
            	game.DoUpdate(_updateGameTime);
			}
		}
		
		protected override void OnVisibleChanged(EventArgs e)
		{			
			base.OnVisibleChanged(e);	
		}
		
		protected override void OnWindowStateChanged(EventArgs e)
		{		
			base.OnWindowStateChanged(e);	
		}
		
		#endregion
				
		#region UIVIew Methods	
		[Export("LongPressGestureRecognizer")]
		public void LongPressGestureRecognizer (UILongPressGestureRecognizer sender)
		{
			var enabledGestures = TouchPanel.EnabledGestures;
			if ((enabledGestures & GestureType.Hold) != 0)
			{
				TouchPanel.GestureList.Enqueue(new GestureSample(GestureType.Hold, new TimeSpan(_now.Ticks), new Vector2 (sender.LocationInView (sender.View)), new Vector2 (sender.LocationInView (sender.View)), new Vector2(0,0), new Vector2(0,0)));
			}
		}
		
		
		[Export("PanGestureRecognizer")]
		public void PanGestureRecognizer (UIPanGestureRecognizer sender)
		{
			
			var enabledGestures = TouchPanel.EnabledGestures;
			if ((enabledGestures & GestureType.FreeDrag) != 0)
			{			
				TouchPanel.GestureList.Enqueue(new GestureSample(GestureType.FreeDrag, new TimeSpan(_now.Ticks), new Vector2 (sender.LocationInView (sender.View)), new Vector2(0,0), new Vector2 (sender.TranslationInView(sender.View)), new Vector2(0,0)));
			}
		}
			
		[Export("PinchGestureRecognizer")]
		public void PinchGestureRecognizer (UIPinchGestureRecognizer sender)
		{
			var enabledGestures = TouchPanel.EnabledGestures;
			if ((enabledGestures & GestureType.Pinch) != 0)
			{
				TouchPanel.GestureList.Enqueue(new GestureSample(GestureType.Pinch, new TimeSpan(_now.Ticks), new Vector2 (sender.LocationOfTouch(0,sender.View)), new Vector2 (sender.LocationOfTouch(1,sender.View)), new Vector2(0,0), new Vector2(0,0)));
			}
		}
		
		
		[Export("RotationGestureRecognizer")]
		public void RotationGestureRecognizer (UIRotationGestureRecognizer sender)
		{
			var enabledGestures = TouchPanel.EnabledGestures;
			if ((enabledGestures & GestureType.Rotation) != 0)
			{
				TouchPanel.GestureList.Enqueue(new GestureSample(GestureType.Rotation, new TimeSpan(_now.Ticks), new Vector2 (sender.LocationInView (sender.View)), new Vector2 (sender.LocationInView (sender.View)), new Vector2(0,0), new Vector2(0,0)));
			}
		}
		
		[Export("SwipeGestureRecognizer")]
		public void SwipeGestureRecognizer (UISwipeGestureRecognizer sender)
		{
			var enabledGestures = TouchPanel.EnabledGestures;
			if ((enabledGestures & GestureType.Flick) != 0)
			{
				TouchPanel.GestureList.Enqueue(new GestureSample(GestureType.Flick, new TimeSpan(_now.Ticks), new Vector2 (sender.LocationInView (sender.View)), new Vector2 (sender.LocationInView (sender.View)), new Vector2(0,0), new Vector2(0,0)));		
			}
		}
		
		[Export("TapGestureRecognizer")]
		public void TapGestureRecognizer (UITapGestureRecognizer sender)
		{
			var enabledGestures = TouchPanel.EnabledGestures;
			if ((enabledGestures & GestureType.Tap) != 0)
			{
				TouchPanel.GestureList.Enqueue(new GestureSample(GestureType.Tap, new TimeSpan(_now.Ticks), new Vector2 (sender.LocationInView (sender.View)), new Vector2 (sender.LocationInView (sender.View)), new Vector2(0,0), new Vector2(0,0)));
			}
		}
		
		private void FillTouchCollection(NSSet touches)
		{
			var enabledGestures = TouchPanel.EnabledGestures;
			if ( enabledGestures == GestureType.None )
			{
				UITouch []touchesArray = touches.ToArray<UITouch>();
				
				for (int i=0; i < touchesArray.Length;i++)
				{
					
					//Get IOS touch
					UITouch touch = touchesArray[i];
					
					//Get position touch
					Vector2 position = new Vector2 (touch.LocationInView (touch.View));
					Vector2 translatedPosition = GetOffsetPosition(position, true);
					
					TouchLocation tlocation;
					TouchCollection collection = TouchPanel.Collection;
					int index;
					switch (touch.Phase)
					{
						case UITouchPhase.Stationary:
						case UITouchPhase.Moved:
							index = collection.FindById(touch.Handle.ToInt32(), out tlocation);
							if (index >= 0)
						    {
								tlocation.State = TouchLocationState.Moved;
								tlocation.Position = translatedPosition;
								collection[index] = tlocation;
							}
							break;
						case UITouchPhase.Began	:	
							tlocation = new TouchLocation(touch.Handle.ToInt32(), TouchLocationState.Pressed, translatedPosition);
							collection.Add(tlocation);	
							break;
						case UITouchPhase.Ended	:
							index = collection.FindById(touch.Handle.ToInt32(), out tlocation);
							if (index >= 0)
							{
								tlocation.State = TouchLocationState.Released;							
								collection[index] = tlocation;
							}
							break;
						case UITouchPhase.Cancelled:
							index = collection.FindById(touch.Handle.ToInt32(), out tlocation);
							if (index >= 0)
							{
								tlocation.State = TouchLocationState.Invalid;
								collection[index] = tlocation;
							}
							break;
						default :
							break;					
					}
				}
			}
			
		}
		
		internal Vector2 GetOffsetPosition(Vector2 position, bool useScale)
		{
			
			Vector2 translatedPosition;
			if (useScale)
			{
				translatedPosition = position * UIScreen.MainScreen.Scale;
			}
			else 
			{
				translatedPosition = position;
			}
					
			switch (CurrentOrientation)
			{
				case DisplayOrientation.Portrait :
				{																		
					break;
				}
				
				case DisplayOrientation.LandscapeRight :
				{				
					translatedPosition = new Vector2( ClientBounds.Height - translatedPosition.Y, translatedPosition.X );							
					break;
				}
				
				case DisplayOrientation.LandscapeLeft :
				{							
					translatedPosition = new Vector2( translatedPosition.Y, ClientBounds.Width - translatedPosition.X );							
					break;
				}
				
				case DisplayOrientation.PortraitUpsideDown :
				{				
					translatedPosition = new Vector2( ClientBounds.Width - translatedPosition.X, ClientBounds.Height - translatedPosition.Y );							
					break;
				}
			}
			
			return translatedPosition;
		}
		
		public override void TouchesBegan (NSSet touches, UIEvent evt)
		{
			base.TouchesBegan (touches, evt);
			
			FillTouchCollection(touches);
			
			GamePad.Instance.TouchesBegan(touches,evt);	
		}
		
		public override void TouchesEnded (NSSet touches, UIEvent evt)
		{
			base.TouchesEnded (touches, evt);
			
			FillTouchCollection(touches);	
			
			GamePad.Instance.TouchesEnded(touches,evt);								
		}
		
		public override void TouchesMoved (NSSet touches, UIEvent evt)
		{
			base.TouchesMoved (touches, evt);
			
			FillTouchCollection(touches);
			
			GamePad.Instance.TouchesMoved(touches,evt);
		}

		public override void TouchesCancelled (NSSet touches, UIEvent evt)
		{
			base.TouchesCancelled (touches, evt);
			
			FillTouchCollection(touches);
			
			GamePad.Instance.TouchesCancelled(touches,evt);
		}

		#endregion
						
		public string ScreenDeviceName 
		{
			get 
			{
				throw new System.NotImplementedException ();
			}
		}

		public Rectangle ClientBounds 
		{
			get 
			{
				return clientBounds;
			}
		}
		
		public bool AllowUserResizing 
		{
			get 
			{
				return false;
			}
			set 
			{
				// Do nothing; Ignore rather than raising and exception
			}
		}	
		
		private DisplayOrientation _currentOrientation;
		public DisplayOrientation CurrentOrientation 
		{ 
			get
            {
                return _currentOrientation;
            }
            internal set
            {
                if (value != _currentOrientation)
                {
                    _currentOrientation = value;
                    if (OrientationChanged != null)
                    {
                        OrientationChanged(this, EventArgs.Empty);
                    }
                }
            }
		}

		public event EventHandler<EventArgs> OrientationChanged;
		public event EventHandler ClientSizeChanged;
		public event EventHandler ScreenDeviceNameChanged;
    }
}

