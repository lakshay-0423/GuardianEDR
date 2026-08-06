const LoadingState = ({ fullScreen = false, label = 'Loading' }) => (
  <div className={fullScreen ? 'loading-state loading-state--fullscreen' : 'loading-state'}>
    <span className="loading-state__spinner" aria-hidden="true" />
    <span>{label}</span>
  </div>
);

export default LoadingState;

