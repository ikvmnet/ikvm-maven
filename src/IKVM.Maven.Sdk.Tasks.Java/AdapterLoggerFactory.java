package IKVM.Maven.Sdk.Tasks.Java;

import org.slf4j.*;

public class AdapterLoggerFactory implements ILoggerFactory {

    public static ILoggerFactory LoggerFactory;

    final ILoggerFactory factory;

    public AdapterLoggerFactory(ILoggerFactory factory) {
        this.factory = factory;
    }

    @Override
    public final Logger getLogger(final String name) {
        return factory.getLogger(name);
    }
    
}
