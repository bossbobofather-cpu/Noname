using MyProject.DefenseGame.Domain;

namespace MyProject.DefenseGame.Domain.AI
{
    /// <summary>
    /// 디펜스 게임 엔티티의 인공지능(의사결정) 인터페이스입니다.
    /// </summary>
    public interface IDefenseAI
    {
        /// <summary>
        /// AI 로직을 수행합니다. 매 틱마다 호출됩니다.
        /// </summary>
        /// <param name="entity">AI가 제어하는 엔티티</param>
        /// <param name="deltaTime">델타 타임</param>
        void Update(DefenseEntity entity, float deltaTime);
    }
}
