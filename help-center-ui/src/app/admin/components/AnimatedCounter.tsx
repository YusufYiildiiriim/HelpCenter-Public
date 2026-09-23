"use client";

import React, { useEffect, useState } from "react";
import { animate } from "framer-motion";

interface AnimatedCounterProps {
  value: number;
  duration?: number;
  delay?: number;
  className?: string;
  suffix?: string;
}

export const AnimatedCounter: React.FC<AnimatedCounterProps> = ({
  value,
  duration = 1.4,
  delay = 0,
  className,
  suffix = "",
}) => {
  const [display, setDisplay] = useState(0);

  useEffect(() => {
    if (value === null || value === undefined || Number.isNaN(value)) return;
    const controls = animate(0, value, {
      duration,
      delay,
      ease: [0.22, 1, 0.36, 1],
      onUpdate: (v) => setDisplay(Math.round(v)),
    });
    return () => controls.stop();
  }, [value, duration, delay]);

  return (
    <span className={className}>
      {display.toLocaleString("tr-TR")}
      {suffix}
    </span>
  );
};
