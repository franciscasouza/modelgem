import { PatternCanvasView } from "@/components/patterns/PatternCanvasView";

export default async function PatternCanvasPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  return <PatternCanvasView patternId={id} />;
}
