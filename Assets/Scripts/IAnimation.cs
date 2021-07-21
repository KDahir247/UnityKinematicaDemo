using Unity.Kinematica;

public interface IAnimation
{
	IAnimation OnUpdate(float deltaTime);


	//Will specify which who will get control of the contract
	bool OnChange(ref MotionSynthesizer synthesizer);
}