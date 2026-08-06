const StatCard = ({ label, value, tone }) => (
  <article className={`stat-card stat-card--${tone}`}>
    <p>{label}</p>
    <strong>{value}</strong>
  </article>
);

export default StatCard;
